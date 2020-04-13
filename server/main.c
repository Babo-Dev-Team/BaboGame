#include <string.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdio.h>
#include <ctype.h>
#include <pthread.h>
#include "json.h"
#include <time.h>

#include "BBDD_Handler.h"
#include "globals.h"
#include "connected_list.h"
#include "game_table.h"

#define NMBR_THREADS 100

//------------------------------------------------------------------------------
// DATA STRUCTS
//------------------------------------------------------------------------------
// Arugments que es passen a cada thread
typedef struct ThreadArgs{
	ConnectedList* connectedList;			// punter a la llista de connectats
	ConnectedUser* connectedUser;			// punter a l'usuari que gestiona el thread 
	GameTable* gameTable;					// punter a la taula de partides
	int freespace;
}ThreadArgs;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
// THREAD FUNCTIONS
//------------------------------------------------------------------------------

//Thread del client
void* attendClient (ThreadArgs* threadArgs)
{	
	int err = BBDD_connect();
	int sock_conn, request_length;
	
	// Punters als paràmetres del thread: connectedUser és l'usuari que gestiona,
	// connectedList i gameTable són les estructures globals que contenen els usuaris i les partides
	//ThreadArgs* threadArgs = (ThreadArgs*) args;
	ConnectedUser* connectedUser = threadArgs->connectedUser;
	ConnectedList* connectedList = threadArgs->connectedList;
	GameTable* gameTable = threadArgs->gameTable;
	
	// punters a un usuari de partida i a una partida. Si l'usuari a qui presta
	// servei el thread crea una partida o s'uneix a una partida existent,
	// aquests punters s'assignen a l'usuari i partida corresponents.
	// Així evitem búsquedes excessives a la taula de partides.
	PreGameUser* preGameUser;
	PreGameState* preGame;
	
	// guardem el socket en una variable local
	sock_conn = connectedUser->socket;
	
	char username[USRN_LENGTH];
	char password[PASS_LENGTH];
	char gameName[GAME_LEN];
	
	char request[CLIENT_REQ_LEN];
	char response[SERVER_RSP_LEN];
	
	int disconnect = 0;
	while(!disconnect)
	{
		// Ahora recibimos el mensaje, que dejamos en request
		// no cal bloquejar la llista, doncs user encara no en forma part
		request_length = read(sock_conn, request, sizeof(request));
		printf ("Recibido\n");
		
		
		// marcamos el final de string
		request[request_length]='\0';
		
		char *p = strtok(request, "/");
		int request_code =  atoi(p); // sacamos el request_code del request
		printf("Request: %d\n", request_code);
		
		switch (request_code)
		{	
			// request 1 -> Total time played by user query
			// 				client request contains: 	username
			// 				server response contains:	time in HH:MM:SS format	
		case 1:
		{
			char username[USRN_LENGTH];
			strcpy(username, strtok(NULL, "/"));
			printf("User: %s\n", username);
			// realitzar la query
			char* time_played = BBDD_time_played(username);
			strcpy(response, time_played);
			free(time_played);
			break;
		}
		
		// request 2 -> global player ranking
		// 				client request contains: 	nothing
		// 				server response contains:	n_pairs/each user*games won pair separated by '/'			
		case 2:
		{
			// realitzar la query
			char* ranking_str = BBDD_ranking();
			strcpy(response, ranking_str);
			free(ranking_str);
			//strcpy(response, "test response 2");
			break;
		}
		
		// request 3 -> Characters used in a game by each user query
		// 				client request contains: 	the name of the game
		// 				server response contains:	n_pairs/each user*character pair separated by '/'
		case 3:
		{
			char game_id[GAME_ID_LENGTH];
			strcpy(game_id, strtok(NULL, "/"));
			
			// realitzar la query
			char* characters_str = BBDD_find_characters(game_id);
			strcpy(response, characters_str);
			free(characters_str);
			break;
		}
		
		
		// request 4 -> login			
		// 				client request contains: 	the login user and passwd
		// 				server response contains:	OK, FAIL	
		case 4:
		{
			strcpy(username, strtok(NULL, "/"));
			strcpy(password, strtok(NULL, "/"));
			printf("User: %s\n", username);
			printf("Password: %s\n", password);
			// realitzar la query
			int id = BBDD_check_login(username, password);
			if(id >= 0)
			{
				// si login OK, omplim els atributs del user
				// i el posem a la llista de connectats.
				// No cal bloquejar la llista, doncs user encara no en forma part
				connectedUser->id = id;
				strcpy(connectedUser->username, username);
				
				// Aqui hem de bloquejar per afegir user a la llista
				// pero ja ho fa la AddConnected
				int err = AddConnected(connectedList, connectedUser);
				if (!err)
				{
					strcpy(response, "OK");					
				}
				else strcpy(response, "FAIL");	
			}
			else 
			{
				strcpy(response, "FAIL");
				disconnect = 1; // close connection to let client try again
			}				
			break;
		}
		
		// request 5 -> Sign Up			
		// 				client request contains: 	the new user and passwd
		// 				server response contains:	OK, FAIL	
		case 5:
		{
			strcpy(username, strtok(NULL, "/"));
			strcpy(password, strtok(NULL, "/"));
			printf("User: %s\n", username);
			printf("Password: %s\n", password);
			// realitzar la query
			int id = BBDD_add_user(username, password);
			if(id >= 0)
			{
				// si sign up OK, afegim els parametres i posem l'usuari a la
				// llista de connectats.
				connectedUser->id = id;
				strcpy(connectedUser->username, username);
				
				// Afegim l'usuari del thread a la llista de connectats. 
				// A partir d'aquí, connectedUser comparteix mutex amb la connectedList
				int err = AddConnected(connectedList, connectedUser);
				strcpy(response, "OK");
			}
			else 
			{
				strcpy(response, "USED");
			}
			break;
		}
		
		case 6:
		{
			json_object* listJson = connectedListToJson(connectedList);
			//strcpy(response, json_object_to_json_string_ext(listJson, JSON_C_TO_STRING_PRETTY));
			strcpy(response, json_object_to_json_string(listJson));
			
			// DESTRUIR LLISTA JSON!!!
			// COMPROVAR SI LA LLIBRERIA DISPOSA D'UN METODE PER ELIMINAR json_object
			free(listJson);
			
			break;
		}
		
		// crear partida
		// l'usuari especifica nom partida
		// es retorna si OK: id partida, token, MAX_GAME_USRCOUNT
		// es retorna un JSON amb l'estat de la partida (serialitzar PreGameState)
		// 7/partida1 -> JSON PreGameState o EXISTS
		case 7:
		{
			strcpy(gameName, strtok(NULL, "/"));
			printf("Create Game: %s\n", gameName);
			if (GameNameAvailable(gameTable, gameName))
			{
				printf("Game is available\n");
				// creem la partida i l'assignem a preGame
				preGame = CreateGame(connectedUser, gameName);
				printf("Create game OK\n");
				// si l'assignació a la taula és correcte, ja tenim un id de partida
				// vàlid i podem assignar també l'usuari. A partir d'aquí, la partida i els
				// seus usaris comparteixen mutex amb la taula de partides.
				// si la partida no es pot crear, la funció ja esborra la partida
				int err = AddGameToGameTable(gameTable, preGame);
				if (err == -2)
				{
					printf("Crear partida FAIL: EXISTS\n");
					strcpy(response, "EXISTS");
				}
				else if (err == -1)
				{
					printf("Crear partida FAIL: TABLE FULL\n");
					strcpy(response, "FULL");
				}
				else
				{
					printf("Crear partida OK\n");
					pthread_mutex_lock(preGame->game_mutex);
					preGameUser = preGame->creator;
					pthread_mutex_unlock(preGame->game_mutex);
					// TODO: retornar partida per JSON
					strcpy(response, "OK");
				}
			}
			else 
			{
				printf("Crear partida FAIL: EXISTS\n");
				strcpy(response, "EXISTS");
			}

			break;
		}
		
		// llista de partides: l'usuari vol la taula de partides
		// es retornara un JSON
		case 8:
		{
			json_object* gameTableJson = GameTableToJson(gameTable);
			strcpy(response, json_object_to_json_string_ext(gameTableJson, JSON_C_TO_STRING_PRETTY));
			//strcpy(response, json_object_to_json_string(listJson));
			
			// DESTRUIR LLISTA JSON!!!
			// COMPROVAR SI LA LLIBRERIA DISPOSA D'UN METODE PER ELIMINAR json_object
			free(gameTableJson);
			
			break;
		}
		
		// join partida: l'usuari demana unir-se a partida pel nom.
		// si s'accepta, se li retorna PreGameState per JSON.
		// si la partida no existeix o esta en curs, no se'l deixa entrar.
		case 9:
		{
			break;
		}
		
		// el creador d'una partida indica que comenci la partida:
		// es posa el estat de la partida en actiu a PreGameState.
		// s'inicialitzen estructures per contenir l'estat de la partida real.
		// aquí, o mantenim els threads oberts i passem a una funcionalitat de partida des del propi thread de cada usuari,
		// on caldria potser fer servir variables des de main per emmagatzemar l'estat de la partida per a que siguin accessibles
		// per tots els threads, on es podria posar el id de la partida en una llista de partides actives i tots els threads
		// que busquin la seva partida per id i hi accedeixin per referència
		// Per fer això caldria que el client Forms pugues passar els paràmetres de connexió al client MonoGame.
		
		// o bé parem tots els threads dels usuaris d'aquesta partida i esperem una nova connexió. en aquesta, l'usuari es loguejarà amb
		// username i token de partida que haurem passat a monogame. Llavors, crearem nous threads, un per user i un thread de gestió de partida
		// els threads de user s'encarreguen de rebre updates dels clients i enviar updates als clients. el thread de gestió
		// calcula les colisions i altres dinàmiques de partida. Això tb es podria fer amb la 1a opció, caldria crear un nou thread de gestió
		
		// la diferència més que res és en si incorporem els protocols de comunicació servidor client in-game en aquesta funció,
		// o bé creem nous threads amb la seva funció específica per gestionar la partida en execució.
		case 10:
		{
			break;
		}
			
		
		// request 0 -> Disconnect	
		// TODO: S'ha d'eliminar el user de la llista de connectats!!!
		case 0:
			// Se acabo el servicio para este cliente
			disconnect = 1;
			break;
			
		default:
			printf("Error. request_code no reconocido.\n");
			break;
		}
		
		// Enviamos response siempre que no se haya recibido request de disconnect
		if(request_code)
		{
			printf ("%s = %s\n", threadArgs->connectedUser->username,response);
			write (sock_conn, response, strlen(response));	
		}
	}
	
	close(sock_conn);
	DelConnectedByName(connectedList, connectedUser->username);
	threadArgs->connectedUser=NULL; //El punter de l'usuari esborrat ara val NULL	
	//Acabar el thread
	pthread_exit(0);
}
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// MAIN FUNCTION
//------------------------------------------------------------------------------
int main(int argc, char *argv[])
{		
	// iniciem el seed per generar aleatorietat
	srand(time(NULL));
	
	int sock_conn, sock_listen;
	struct sockaddr_in serv_addr;

	
	// INICIALITZACIONS
	// Obrim el socket
	if ((sock_listen = socket(AF_INET, SOCK_STREAM, 0)) < 0)
		printf("Error creant socket");
	
	// Fem el bind al port
	memset(&serv_addr, 0, sizeof(serv_addr)); // inicialitza a zero serv_addr
	serv_addr.sin_family = AF_INET;
	
	// asocia el socket a cualquiera de las IP de la maquina. 
	// htonl formatea el numero que recibe al formato necesario
	serv_addr.sin_addr.s_addr = htonl(INADDR_ANY);
	// escucharemos en el port 9050
	serv_addr.sin_port = htons(9000);
	if (bind(sock_listen, (struct sockaddr *) &serv_addr, sizeof(serv_addr)) < 0)
		printf ("Error al bind");
	//La cola de requestes pendientes no podr? ser superior a 4
	if (listen(sock_listen, 2) < 0)
		printf("Error en el Listen");
	
	//CONNEXIÓ
	int i;
	pthread_t thread[NMBR_THREADS];
	
	// Llista de connectats.
	// creem el mutex de la llista de connectats
	// que també utilitzaran els connected users
	// la funció CreateConnectedList crea una llista de connectats i 
	// li passem el mutex de la llista.
	pthread_mutex_t* mutexConnectedList = malloc(sizeof(pthread_mutex_t));
	pthread_mutex_init(mutexConnectedList, NULL);
	ConnectedList* connectedList = CreateConnectedList(mutexConnectedList);
	
	
	// Taula de partides
	// Creem el mutex de la taula de partides
	// que també utilitzaran els PreGameState i els PreGameUser
	pthread_mutex_t* mutexGameTable = malloc(sizeof(pthread_mutex_t));
	pthread_mutex_init(mutexGameTable, NULL);
	GameTable* gameTable = CreateGameTable(mutexGameTable);
	
	//GameTable* gameTable = malloc(sizeof(GameTable));
	//gameTable->gameCount = 0;
	//gameTable->game_table_mutex = malloc(sizeof(pthread_mutex_t));
	//pthread_mutex_init(gameTable->game_table_mutex, NULL);
	
	// passem a threadArgs els punters a la taula de partides
	// i la llista d'usuaris connectats, iguals per cada element.
	ThreadArgs threadArgs[NMBR_THREADS];
	for(i = 0; i < NMBR_THREADS; i++)
	{
		threadArgs[i].connectedList = connectedList;
		threadArgs[i].gameTable = gameTable;
		threadArgs[i].connectedUser = NULL;
	}

	// Atenem infinites peticions
	int Iterator=0;
	int freespace=0;
	for(;;)
	{
		Iterator = 0;
		freespace = 0;
		if (threadArgs[Iterator].connectedList->number>=CNCTD_LST_LENGTH)
		{
			printf ("No queda espai per a més jugadors\n");
		}
		else
		{
		while ((Iterator<CNCTD_LST_LENGTH)&&(freespace==0))
		{
			if (threadArgs[Iterator].connectedUser==NULL)
			{
				freespace=1;
				threadArgs[Iterator].connectedUser = NULL;			
			}
			else
				Iterator++;
		}
		if(freespace==0)
			printf("No se puede escuchar más gente");
		else
		{
			printf ("Escuchando\n");	
			
			//sock_conn es el socket que usaremos para este cliente
			sock_conn = accept(sock_listen, NULL, NULL);
			printf ("He recibido conexi?n\n");
			
			// Creem el l'usuari per cada thread que s'instancia i el posem
			// a l'element de l'array threadArgs que es passa al thread
			threadArgs[Iterator].connectedUser = CreateConnectedUser();
			threadArgs[Iterator].connectedUser->socket = sock_conn;
			
			// creem el thread
			pthread_create(&thread[Iterator], NULL, attendClient, &threadArgs[Iterator]);
			printf("Iterator: %d\n", Iterator);
		}
		
		}
	}
	
	// Alliberem recursos
	/*for(i = 0; i < NMBR_THREADS; i++)
	{
		free(threadArgs[i].connectedUser);
	}*/
	
	DeleteConnectedList(connectedList);  // eliminem la llista de connectats i tots els usuaris
	DeleteGameTable(gameTable);		// eliminem la taula de partides i totes les partides
	pthread_mutex_destroy(mutexConnectedList);
	pthread_mutex_destroy(gameTable->game_table_mutex);
}
//------------------------------------------------------------------------------



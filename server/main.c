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

#define NMBR_THREADS 100
#define MAX_GAME_USRCOUNT 6
#define TOKEN_LEN 32
#define MAX_GAMES 256

//----------------------------------------------------------

typedef struct PreGameThreadArgs{
	ConnectedList* connectedList;
	ConnectedUser* connectedUser;
}PreGameThreadArgs;

// usuari dins la pre-partida
typedef struct PreGameUser{
	int id;
	int socket;
	char username[USRN_LENGTH];
	char charname[CHAR_LEN];
}PreGameUser;

// estat de la pre-partida
typedef struct PreGameState{
	int gameId;								// id de la partida
	int userCount;							// nombre de participants
	char gameName[GAME_LEN];				// nom de la partida
	PreGameUser* creator;					// creador de la partida
	PreGameUser* users[MAX_GAME_USRCOUNT];	// llista usuaris a la partida
	char accessToken[TOKEN_LEN]; 			// token per accedir a la partida des de monogame
	int inGame;								// bool per saber si estem jugant o esperant
	int softKill;					    	// bool per indicar partida acabada i pot ser eliminada
	pthread_mutex_t game_mutex;				// mutex de la pre-partida
	
}PreGameState;

// taula de partides (informativa), utilitza soft delete
// és una taula de PreGameState
typedef struct GameTable{
	PreGameState* createdGames[MAX_GAMES];
	int gameCount;
	pthread_mutex_t game_table_mutex;
}GameTable;

// genera una seqüència aleatòria que utilitzem com a token d'accés a la partida
void token_gen(char token[TOKEN_LEN])
{
	int size = TOKEN_LEN - 1;
	const char charset[] = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJK0123456789";
	for (int i = 0; i < size; i++) 
	{
		int key = rand() % (int) (sizeof charset - 1);
		token[i] = charset[key];
	}
	token[TOKEN_LEN] = '\0';
}

// afegim un PreGameUser a la llista d'usuaris de la partida
int AddPreGameUser(PreGameState* gameState, PreGameUser* user)
{
	pthread_mutex_lock(&gameState->game_mutex);
	if(gameState->userCount < MAX_GAME_USRCOUNT)
	{
		int pos = gameState->userCount;
		gameState->users[pos] = user;
		gameState->userCount++;
		pthread_mutex_unlock(&gameState->game_mutex);
		return 0;
	}
	else
	{
		pthread_mutex_unlock(&gameState->game_mutex);
		return -1;
	}
}

// eliminem un usuari de la partida (l'usuari abandona el PreGame)
int DeletePreGameUser(PreGameState* gameState, PreGameUser* user)
{
	pthread_mutex_lock(&gameState->game_mutex);
	int userCount = gameState->userCount;
	int pos;
	int found = 0;
	int i = 0;
	while (i < userCount && !found)
	{
		if(gameState->users[i] == user)
		{
			found = 1;
			pos = i;
		}
		else 
		   i++;
	}
	int ret;
	if (found)
	{
		free(gameState->users[pos]); // alliberem el PreGameUser de la memoria
		for(int j = pos; j < userCount - 1; j++)
		{
			gameState->users[j] = gameState->users[j + 1];
		}
		gameState->userCount--;
		ret = 0;
	}
	else 
		ret = -1;
	pthread_mutex_unlock(&gameState->game_mutex);
	return ret;
}

// assignació de personatge a l'usuari per a una partida (PreGame)
int PreGameAssignChar(PreGameState* gameState, char username[USRN_LENGTH], char charname[CHAR_LEN])
{
	pthread_mutex_lock(&gameState->game_mutex);
	int userCount = gameState->userCount;
	int pos;
	int i = 0;
	int found = 0;
	while (i < userCount && !found)
	{
		if(!strcmp(gameState->users[i]->username, username))
		{
			found = 1;
			pos = i;
		}
		else 
			i++;
	}
	int ret;
	if (found)
	{
		strcpy(gameState->users[i]->charname, charname);
		ret = 1;
	}
	else 
		ret = 0;
	pthread_mutex_unlock(&gameState->game_mutex);
	return ret;
}

// crea un usuari PreGameUser a partir d'un ConnectedUser
// per poder afegir-lo a la partida
PreGameUser* CreatePreGameUser(ConnectedUser* user)
{
	PreGameUser* preGameUser = malloc(sizeof(PreGameUser));
	pthread_mutex_lock(&user->user_mutex);
	preGameUser->socket = user->socket;
	preGameUser->id = user->id;
	strcpy(preGameUser->username, user->username);
	pthread_mutex_unlock(&user->user_mutex);
	return preGameUser;
}

// creem una partida en estat PreGame (sala de partida on es poden unir altres jugadors)
// aquesta funció inicialitza PreGameState amb el creador de la partida
PreGameState* CreateGame(ConnectedUser* user, char name[GAME_LEN])
{
	// no ens cal bloquejar el preGameState perque som els únics utilitzant-lo
	PreGameState* preGameState = malloc(sizeof(PreGameState));
	preGameState->inGame = 0;
	preGameState->softKill = 0;
	strcpy(preGameState->gameName, name); //TODO: comprovar que no estigui ocupat!!!! millor des del scope on es cridi aquesta funcio per no passar tota la taula
	token_gen(preGameState->accessToken);
	preGameState->creator = CreatePreGameUser(user);
	AddPreGameUser(preGameState, preGameState->creator);
	return preGameState;
}



//Thread del client
void* attendClient (void* args)
{	
	int err = BBDD_connect();
	int sock_conn, request_length;
	PreGameThreadArgs* threadArgs = (PreGameThreadArgs*) args;
	
	// Punters al usuari que gestiona el thread i a la llista
	ConnectedUser* connectedUser = threadArgs->connectedUser;
	ConnectedList* connectedList = threadArgs->connectedList;
	sock_conn = connectedUser->socket;
	
	char username[USRN_LENGTH];
	char password[PASS_LENGTH];
	
	char request[512];
	char response[512];
	
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
			printf("funciona");
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
			//AddConnected(user->list,username,sock_conn);
			// realitzar la query
			int id = BBDD_add_user(username, password);
			if(id >= 0)
			{
				// si sign up OK, afegim els parametres i posem l'usuari a la
				// llista de connectats.
				connectedUser->id = id;
				strcpy(connectedUser->username, username);
				int err = AddConnected(connectedList, connectedUser);
				strcpy(response, "OK");
			}
			else 
			{
				strcpy(response, "USED"); // encara no s'utilitza
			}
			//disconnect = 1; // close connection to let client log in
			//strcpy(response, "test response 3");
			break;
		}
		
		case 6:
		{
			json_object* listJson = connectedListToJson(connectedList);
			//strcpy(response, json_object_to_json_string_ext(listJson, JSON_C_TO_STRING_PRETTY));
			strcpy(response, json_object_to_json_string(listJson));
			
			// DESTRUIR LLISTA JSON!!!
			
			break;
		}
		
		// crear partida
		// l'usuari especifica nom partida
		// es retorna si OK: id partida, token, MAX_GAME_USRCOUNT
		// es retorna un JSON amb l'estat de la partida (serialitzar PreGameState)
		case 7:
		{
			break;
		}
		
		// llista de partides: l'usuari vol la taula de partides
		// es retornara un JSON
		case 8:
		{
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
			printf ("%s\n", response);
			write (sock_conn, response, strlen(response));	
		}
	}
	
	close(sock_conn);
	DelConnectedByName(connectedList, connectedUser->username);
	
	//Acabar el thread
	pthread_exit(0);
}

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
	serv_addr.sin_port = htons(9098);
	if (bind(sock_listen, (struct sockaddr *) &serv_addr, sizeof(serv_addr)) < 0)
		printf ("Error al bind");
	//La cola de requestes pendientes no podr? ser superior a 4
	if (listen(sock_listen, 2) < 0)
		printf("Error en el Listen");
	
	//CONNEXIÓ
	int i;
	pthread_t thread[NMBR_THREADS];
	
	// Llista de connectats
	ConnectedList* connectedList = malloc(sizeof(ConnectedList));
	connectedList->number = 0;
	pthread_mutex_init(&connectedList->mutex, NULL);
	
	// Taula de partides
	GameTable* gameTable = malloc(sizeof(GameTable));
	gameTable->gameCount = 0;
	pthread_mutex_init(&gameTable->game_table_mutex, NULL);
	
	PreGameThreadArgs threadArgs[NMBR_THREADS];
	for(i = 0; i < NMBR_THREADS; i++)
	{
		threadArgs[i].connectedList = connectedList;
		threadArgs[i].connectedUser = malloc(sizeof(ConnectedUser));
	}

	// Atenderemos requestes indefinidamente
	for(i = 0; i < 5; i++)
	{
		printf ("Escuchando\n");	
		
		//sock_conn es el socket que usaremos para este cliente
		sock_conn = accept(sock_listen, NULL, NULL);
		printf ("He recibido conexi?n\n");
		
		threadArgs[i].connectedUser->socket = sock_conn;
		pthread_mutex_init(&threadArgs[i].connectedUser->user_mutex, NULL); // inicialitzem el mutex de l'usuari
		
		pthread_create(&thread[i], NULL, attendClient, &threadArgs[i]);
	}
	
	for(i = 0; i < NMBR_THREADS; i++)
	{
		free(threadArgs[i].connectedUser);
	}
	pthread_mutex_destroy(&connectedList->mutex);
	free(connectedList);
}


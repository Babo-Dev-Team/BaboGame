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

#define SHIVA_PORT 50084

//#define NMBR_THREADS 100

//------------------------------------------------------------------------------
// DATA STRUCTS
//------------------------------------------------------------------------------

// Arguments del global sender (inclosos a thread args amb punter)
typedef struct SenderArgs{
	char globalResponse[SERVER_RSP_LEN];
	//int sockets [CNCTD_LST_LENGTH];
	pthread_mutex_t* sender_mutex;
	pthread_cond_t* sender_signal;
}SenderArgs;

typedef struct GameSenderArgs{
	char gameResponse[SERVER_RSP_LEN];
	int userState;
	PreGameState* game;
	pthread_mutex_t* gameSender_mutex;
	pthread_cond_t* gameSender_signal;
}GameSenderArgs;

// Arugments que es passen a cada thread
typedef struct ThreadArgs{
	ConnectedList* connectedList;			// punter a la llista de connectats
	ConnectedUser* connectedUser;			// punter a l'usuari que gestiona el thread 
	GameTable* gameTable;					// punter a la taula de partides	
	pthread_mutex_t* threadArgs_mutex;	
	SenderArgs* senderArgs;
	GameSenderArgs** gameSenderArgs;
}ThreadArgs;

//------------------------------------------------------------------------------

// funció per enviar a tothom, activa el thread globalSender
void sendToAll (SenderArgs* args, char response[SERVER_RSP_LEN])
{
	// adquirim el lock del sender (alliberat pel pthread_mutex_wait en el seu thread)
	pthread_mutex_lock(args->sender_mutex);
	strcpy(args->globalResponse, response);
	
	// ara indiquem al thread que s'executi
	pthread_cond_signal(args->sender_signal);
	
	pthread_mutex_unlock(args->sender_mutex);
}

//------------------------------------------------------------------------------
// THREAD FUNCTIONS
//------------------------------------------------------------------------------

// funció per enviar a tothom, activa el thread globalSender
void sendToGame (GameSenderArgs* args, char response[SERVER_RSP_LEN], int userState)
{
	// adquirim el lock del sender (alliberat pel pthread_mutex_wait en el seu thread)
	pthread_mutex_lock(args->gameSender_mutex);
	int waitNotified = 0;
	int unlocked = 0;
	
	// si el gameSender no ha processat les dades anteiors però ens deixa
	// adquirie el lock, és perquè encara no ha sortit del pthread_condition_wait.
	// el que fem és anar alliberant el mutex i esperar que el sender adquireixi el mutex
	// i envii la informació. podem llegir userState perquè és un int32 (atomic)
	while (args->userState != -2)
	{
		pthread_mutex_unlock(args->gameSender_mutex);
		unlocked = 1;
		if(!waitNotified)
		{
			printf("sendToGame: Waiting for the game sender to wake up...\n");
			waitNotified = 1;
		}
	}
	if (unlocked)
	{
		pthread_mutex_lock(args->gameSender_mutex);
	}
	args->userState = userState;
	pthread_cond_signal(args->gameSender_signal);
	strcpy(args->gameResponse, response);

	// ara indiquem al thread que s'executi
	pthread_mutex_unlock(args->gameSender_mutex);
}


void* gameSender (void* args)
{
	GameSenderArgs* senderArgs = (GameSenderArgs*) args;
	
	// inicialitzem un array de sockets actius
	int activeSocketList[MAX_GAME_USRCOUNT];
	int userStates[MAX_GAME_USRCOUNT];
	for (int i = 0; i < MAX_GAME_USRCOUNT; i++)
	{
		activeSocketList[i] = -1;
		userStates[i] = -2;
	}
	
	while(1)
	{	
		// hem de bloquejar els senderArgs per a que 
		// pthread_condition_wait s'executi correctament
		pthread_mutex_lock(senderArgs->gameSender_mutex);	
		
		// indiquem posant userState a -2 que estem esperant per rebre noves dades.
		// No farem res fins que sendToGame no ens passi info i un userState valid per enviar
		senderArgs->userState = -2;
		while(senderArgs->userState == -2)
		{
			printf("game sender entering sleep\n");
			// alliberem els args per a que la funcio sendToAll els pugui modificar
			// i esperem que ens indiqui amb el sender_signal que tenim dades per enviar
			if(pthread_cond_wait(senderArgs->gameSender_signal, senderArgs->gameSender_mutex))
			{
				printf("game sender CONDITION WAIT ERROR\n");
			}
		}
		// consultem els sockets actius a la llista de connectats	
		
		// la idea es que durant la partida, el thread de calcul cridi directament la funcio sendtogame.
		printf("game sender waking up\n");
		
		pthread_mutex_lock(senderArgs->game->game_mutex);
		int n_users = senderArgs->game->userCount;
		for(int i = 0; i < n_users; i++)
		{
			activeSocketList[i] = senderArgs->game->users[i]->socket;
			userStates[i] = senderArgs->game->users[i]->userState;
		}
		pthread_mutex_unlock(senderArgs->game->game_mutex);
		strcat(senderArgs->gameResponse, "|");
		
		// enviem a tots els sockets actius
		printf("Sending to game users with User State = %d\n", senderArgs->userState);
		for (int i = 0; i < n_users; i++)
		{		
			if (senderArgs->userState == userStates[i])
			{
				printf ("GAME ID %d NOTIFICATION for %s = %s\n", senderArgs->game->gameId, senderArgs->game->users[i]->username, senderArgs->gameResponse);
				write (activeSocketList[i], senderArgs->gameResponse, strlen(senderArgs->gameResponse));			
			}
		}	
		pthread_mutex_unlock(senderArgs->gameSender_mutex);
	}
	//pthread_mutex_unlock(threadArgs->threadArgs_mutex);
}

// sender globa, envia a tots els connectats.
// fa anar un senyal pthread. El sender estarà inactiu fins que es cridi 
// la funció SendToAll
void* globalSender (void* args)
{
	// bloquegem els threadArgs perquè volem que ningú més accedeixi als ThreadArgs del sender
	pthread_mutex_lock(((ThreadArgs*)args)->threadArgs_mutex);
	ThreadArgs* threadArgs = (ThreadArgs*) args;
	SenderArgs* senderArgs = threadArgs->senderArgs;

	// inicialitzem un array de sockets actius
	int activeSocketList[CNCTD_LST_LENGTH];
	for (int i = 0; i < CNCTD_LST_LENGTH; i++)
	{
		activeSocketList[i] = -1;
	}
	
	while(1)
	{	
		// hem de bloquejar els senderArgs per a que 
		// pthread_condition_wait s'executi correctament
		pthread_mutex_lock(senderArgs->sender_mutex);
		
		// alliberem els args per a que la funcio sendToAll els pugui modificar
		// i esperem que ens andiqui amb el sender_signal que tenim dades per enviar
		pthread_cond_wait(senderArgs->sender_signal, senderArgs->sender_mutex);
		
		// consultem els sockets actius a la llista de connectats
		pthread_mutex_lock(threadArgs->connectedList->mutex);
		int n_users = threadArgs->connectedList->number;
		for(int i = 0; i < n_users; i++)
		{
			activeSocketList[i] = threadArgs->connectedList->connected[i]->socket;
		}
		pthread_mutex_unlock(threadArgs->connectedList->mutex);
		strcat(senderArgs->globalResponse, "|");
		// enviem a tots els sockets actius
		for (int i = 0; i < n_users; i++)
		{			
			printf ("%s = %s\n", "GLOBAL NOTIFICATION: ", senderArgs->globalResponse);
			write (activeSocketList[i], senderArgs->globalResponse, strlen(senderArgs->globalResponse));			
		}
		// desbloquegem per si sortíssim del loop
		pthread_mutex_unlock(senderArgs->sender_mutex);
	}
	pthread_mutex_unlock(threadArgs->threadArgs_mutex);
}

//Thread del client
// DONE: enviar llista de connectats modificada al fer login 
//		Afegir separador | a totes les respostes del server (final de missatge)
// TODO: enviar llista de connectats modificada al desconnectar client,
// 	    Enviar llista de connectats modificada al fer sign-up 
// 		enviar taula de partides modificada com a notif. global
void* attendClient (void* args)
{
	int err = BBDD_connect();
	int sock_conn, request_length;
	
	// inicialitzem un punter a l'element corresponent de l'array de ThreadArgs
	// de la funció main
	pthread_mutex_lock(((ThreadArgs*)args)->threadArgs_mutex);
	ThreadArgs* threadArgs = (ThreadArgs*) args;
	
	// Punters als paràmetres del thread: connectedUser és l'usuari que gestiona,
	// connectedList i gameTable són les estructures globals que contenen els usuaris i les partides
	ConnectedUser* connectedUser = threadArgs->connectedUser;
	ConnectedList* connectedList = threadArgs->connectedList;
	GameTable* gameTable = threadArgs->gameTable;
	GameSenderArgs** gameSenderArgs = threadArgs->gameSenderArgs;

	// guardem el socket en una variable local
	sock_conn = connectedUser->socket;
	
	// A partir d'aquí ja no ens cal bloquejar l'array threadArgs per accedir als paràmetres del thread
	// perquè ho farem a través dels punters que acabem d'inicialitzar. Ja no passem per l'array
	// threadArgs per dereferenciar els objectes connectedUser, connectedList, gameTable o el socket. 
	// les Structs connectedList i gameTable ja tenen els seus mutex, que han estat incialitzats a main
	// i que ja uitilizen les funcions que modifiquen aquestes estructures.
	pthread_mutex_unlock(threadArgs->threadArgs_mutex);
	
	// punters a un usuari de partida i a una partida. Si l'usuari a qui presta
	// servei el thread crea una partida o s'uneix a una partida existent,
	// aquests punters s'assignen a l'usuari i partida corresponents.
	// Així evitem búsquedes excessives a la taula de partides.
	PreGameUser* preGameUser;
	PreGameState* preGame;
	
	char username[USRN_LENGTH];
	char password[PASS_LENGTH];
	char gameName[GAME_LEN];
	
	char request[CLIENT_REQ_LEN];
	char request_string[CLIENT_REQ_LEN];
	char response[SERVER_RSP_LEN];
	char globalResponse[SERVER_RSP_LEN];
	char gameNotification[SERVER_RSP_LEN];
	
	// aquests flag indica si cal enviar alguna notificació de forma global
	int globalSend;
	int gameSend;
	int gameSendUserState;
	
	// aquest flag indica si cal enviar resposta a l'usuari que fa el request
	int userSend;
	
	int gameId;
	
	int disconnect = 0;
	while(!disconnect)
	{
		// Ahora recibimos el mensaje, que dejamos en request
		// no cal bloquejar la llista, doncs user encara no en forma part
		request_length = read(sock_conn, request, sizeof(request));
		printf ("Recibido\n");
		
		
		// marcamos el final de string
		request[request_length]='\0';
		strcpy(request_string, request);
		char *p = strtok(request, "/");
		int request_code =  atoi(p); // sacamos el request_code del request
		printf("Request: %d\n", request_code);
		
		// resetegem l'estat de les strings i flags de resposta
		strcpy(response, "");
		strcpy(gameNotification, "");
		strcpy(globalResponse, "");
		
		// per defecte, habilitem la resposta a l'usuari i deshabilitem respostes globals
		userSend = 1;
		globalSend = 0;
		gameSend = 0;
		gameSendUserState = -1;
		
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
			strcpy(response, "1/");
			strcat(response, time_played);
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
			strcpy(response, "2/");
			strcat(response, ranking_str);
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
			strcpy(response, "3/");
			strcat(response, characters_str);
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
					strcpy(response, "4/");
					strcat(response, "OK");	
					
					json_object* listJson = connectedListToJson(connectedList);
					
					// si modifiquem la llista, cal enviar la nova llista de forma global
					strcpy(globalResponse, "6/");
					strcat(globalResponse, json_object_to_json_string(listJson));
					globalSend = 1;
					
					// COMPROVAR SI LA LLIBRERIA DISPOSA D'UN METODE PER ELIMINAR json_object
					free(listJson);
				}
				else 
				{
					strcpy(response, "4/");
					strcat(response, "FAIL");	
				}
			}
			else 
			{
				strcpy(response, "4/");
				strcat(response, "FAIL");
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
				strcpy(response, "5/");
				strcat(response, "OK");
				
				json_object* listJson = connectedListToJson(connectedList);
				
				// si modifiquem la llista, cal enviar la nova llista de forma global
				strcpy(globalResponse, "6/");
				strcat(globalResponse, json_object_to_json_string(listJson));
				globalSend = 1;
				
				// COMPROVAR SI LA LLIBRERIA DISPOSA D'UN METODE PER ELIMINAR json_object
				free(listJson);
			}
			else 
			{
				strcpy(response, "5/");				
				strcpy(response, "USED");
			}
			break;
		}
		
		case 6:
		{
			json_object* listJson = connectedListToJson(connectedList);
			//strcpy(response, json_object_to_json_string_ext(listJson, JSON_C_TO_STRING_PRETTY));
			strcpy(response, "6/");
			strcat(response, json_object_to_json_string(listJson));
			
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
			p = strtok(NULL,"/");
			strcpy(gameName, p);
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
				gameId = AddGameToGameTable(gameTable, preGame);
				if (gameId == -2)
				{
					printf("Crear partida FAIL: EXISTS\n");
					strcpy(response, "7/");
					strcat(response, "EXISTS");
				}
				else if (gameId == -1)
				{
					printf("Crear partida FAIL: TABLE FULL\n");
					strcpy(response, "7/");
					strcat(response, "FULL");
				}
				else
				{
					// assignem a preGameUser el creador de la partida,
					// que és l'usuari que gestiona el thread de tipus PreGameUser
					printf("Id partida: %d\n", gameId); 
					printf("Creant partida\n");
					printf("%s\n", request_string);
					pthread_mutex_lock(preGame->game_mutex);
					preGameUser = preGame->creator;
					pthread_mutex_unlock(preGame->game_mutex);
					
					
					// posem els usuaris escollits a la llista jugadors de la partida
					p = strtok(NULL,"/");
					int playersCount = atoi(p);
					printf("Numer of players:%d\n", playersCount);
					
					int i=0;
					for(i=0; i<playersCount;i++)
					{
						p = strtok(NULL,"/");
						
						//Busquem el punter del jugador a invitar en el connectedList
						int playerConnectedListPos = GetConnectedPos(connectedList, p);
						PreGameUser* player;

						if(playerConnectedListPos != -1)
						{
							pthread_mutex_lock(threadArgs->threadArgs_mutex);
							player = CreatePreGameUser(connectedList->connected[playerConnectedListPos]);
							pthread_mutex_unlock(threadArgs->threadArgs_mutex);
						}
						else
					    {
							player = NULL;
							printf("Afegir jugador FAIL: NOCONNECTLIST\n");
						}
						//Afegim el jugador al preGame
						if(player != NULL)
						{
							if(GetPreGameUserPosByName(preGame,player->username) == -1)
							{
								AddPreGameUser(preGame,player);
							}
							else
						    {
								printf("Afegir jugador FAIL: ALREADYEXIST\n");
							}
						}
					}
					
					// Comprovem que hi hagi suficients jugadors i els enviem l'avis
					pthread_mutex_lock(preGame->game_mutex);
					printf("UserCount: %d\n",preGame->userCount);
					pthread_mutex_unlock(preGame->game_mutex);
					if(preGame->userCount > 1)
					{
					
						// TODO: retornar partida per JSON
						printf("Crear partida OK\n");
						strcpy(response, "7/");
						strcat(response, "OK");
						
						// Enviem l'avis als altres membres
						//char notify_group[SERVER_RSP_LEN];
						pthread_mutex_lock(preGame->game_mutex);
						
						// escrivim la notificacio de partida
						sprintf(gameNotification, "9/NOTIFY/%s/%s/",preGame->gameName,preGame->creator->username);
						//printf("GAME GROUP NOTIFICATION: %s\n",gameName);
						pthread_mutex_unlock(preGame->game_mutex);
						
						// passem als arguments del sender la partida que ha de servir
						// com que ho passarem als args amb index igual a la id de la partida,
						// li estem assignant la funcio de fer de sender de la partida N al
						// Sender Thread amb index N dins l'array de threads.
						pthread_mutex_lock(gameSenderArgs[gameId]->gameSender_mutex);
						gameSenderArgs[gameId]->game = preGame;
						pthread_mutex_unlock(gameSenderArgs[gameId]->gameSender_mutex);
						//sendToGame(gameSenderArgs[gameId], gameNotification, 0);
						
						// indiquem que volem que s'envii la notifiacio als usuaris amb estat 0 al final del switch
						gameSend = 1;
						gameSendUserState = 0;
						
						/*for(int i=0;i<preGame->userCount;i++)
						{
							if(preGame->users[i]->userState == 0)
								write(preGame->users[i]->socket, notify_group, strlen(notify_group));
						}*/
					}
					else
				    {
						
						//Eliminem la partida
						//DeleteGameFromTable(gameTable,preGame);
						
						printf("Crear partida FAIL: ALONE\n");
						strcpy(response, "7/");
						strcat(response, "ALONE");
					}
					
				}
			}
			else 
			{
				printf("Crear partida FAIL: EXISTS\n");
				strcpy(response, "7/");
				strcat(response, "EXISTS");
			}

			break;
		}
		
		// llista de partides: l'usuari vol la taula de partides
		// es retornara un JSON
		case 8:
		{
			json_object* gameTableJson = GameTableToJson(gameTable);
			strcpy(response, "8/");
			strcat(response, json_object_to_json_string_ext(gameTableJson, JSON_C_TO_STRING_PRETTY));
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
			printf("%s\n",request_string);
			
			p = strtok(NULL,"/");
			char option [20];
			strcpy(option,p);
			p = strtok(NULL,"/");
			int preGameUserPos;
			
			
			
			//Accepta la peticio d'entrar en el joc
			if(strcmp(option,"ACCEPT") == 0)
			{
				preGame = GetPreGameStateByName(gameTable, p);
				if(preGame != NULL)
				{
					preGameUserPos = GetPreGameUserPosByName(preGame, username);
					
					pthread_mutex_lock(preGame->game_mutex);
					gameId = preGame->gameId;
					printf("Game Id: %d\n", gameId);
					if(preGameUserPos != -1)
					{
						preGameUser = preGame->users[preGameUserPos];
						preGameUser->userState = 1; //Marca la partida com a acceptada
					}
					pthread_mutex_unlock(preGame->game_mutex);
					
					if(preGameUserPos != -1)
					{
						//Missatge per comunicar al usari que acceptava
						printf("%s accepta la partida %s\n", username, p);
						strcpy(response, "9/");
						strcat(response, "ACCEPTED");
						
						//char notify_group [SERVER_RSP_LEN];
						json_object* gameStateJson = GameStateToJson(preGame);
						strcpy(gameNotification, "10/");
						strcat(gameNotification, json_object_to_json_string_ext(gameStateJson, JSON_C_TO_STRING_PRETTY));
						gameSend = 1;
						gameSendUserState = 1;						
						free(gameStateJson);												
					}
					else
					{
						printf("Acceptar partida FAIL: USER ISN'T IN GAME\n");
						strcpy(response, "9/");
						strcat(response, "FAIL");
					}
				
				}
				else
			    {
					printf("Acceptar partida FAIL: GAME DOESN'T EXISTS\n");
					strcpy(response, "9/");
					strcat(response, "FAIL");
				}
				
			}
			//Rebutja la peticio d'entrar en el joc
			else if (strcmp(option, "REJECT") == 0)
			{
				PreGameState* preGameRejected;
				preGameRejected = GetPreGameStateByName(gameTable, p);
				if(preGameRejected != NULL)
				{
					preGameUserPos = GetPreGameUserPosByName(preGameRejected, username);
					pthread_mutex_lock(preGameRejected->game_mutex);
					gameId = preGameRejected->gameId;
					if(preGameUserPos != -1)
					{
						preGameRejected->users[preGameUserPos]->userState = -1; //Marca la partida com a rebutjada
					}
					pthread_mutex_unlock(preGameRejected->game_mutex);
					
					if(preGameUserPos != -1)
					{
						printf("%s rebutja la partida %s\n", username, p);
						strcpy(response, "9/");
						strcat(response, "REJECTED");
			
						if(IamAloneinGame(preGameRejected))
						{
							char creator [USRN_LENGTH];
							strcpy(creator, preGameRejected->creator->username);
							pthread_mutex_lock(preGameRejected->game_mutex);
							printf("%s is alone in %s\n", creator, preGameRejected->gameName);
							char creatorResponse [SERVER_RSP_LEN];
							strcpy(creatorResponse, "12/");
							strcat(creatorResponse, "ALONE|");
							printf("Creator %s = %s\n", creator, creatorResponse);
							write(preGameRejected->creator->socket, creatorResponse, strlen(creatorResponse));
							pthread_mutex_unlock(preGameRejected->game_mutex);							
							DeleteGameFromTable(gameTable,preGameRejected);
							preGame = NULL;
							gameId = -1;
						}
						else 
						{
							json_object* gameStateJson = GameStateToJson(preGameRejected);
							strcpy(gameNotification, "10/");
							strcat(gameNotification, json_object_to_json_string_ext(gameStateJson, JSON_C_TO_STRING_PRETTY));							
							gameSend = 1;
							gameSendUserState = 1;
							free(gameStateJson);
						}
					}
					else
					{
						printf("Rebutjar partida FAIL: USER ISN'T IN GAME\n");
						strcpy(response, "9/");
						strcat(response, "FAIL");
					}
				
				}
				else
				{
					printf("Rebutjar partida FAIL: GAME DOESN'T EXISTS\n");
					strcpy(response, "9/");
					strcat(response, "FAIL");
				}
			}
			
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
		
		//codi perque els usuaris es connectin per xat
		case 11:
		{
			printf("%s\n",request_string);
			
			p = strtok(NULL,"/");
			char message [200];
			strcpy(message,p);
			
			//Missatge per comunicar al usuari que acceptava
			printf("Missatge rebut: %s\n", message);			
			strcpy(gameNotification, "11/");
			pthread_mutex_lock(connectedUser->user_mutex);
			strcat(gameNotification,connectedUser->username);
			pthread_mutex_unlock(connectedUser->user_mutex);
			strcat(gameNotification,"/");
			strcat(gameNotification, message);
			gameSend = 1;
			gameSendUserState = 1;
			userSend = 0;
			break;
		}
		
		//Inici de la partida
		case 12:
		{
			printf("%s\n",request_string);
			
			p = strtok(NULL,"/");
			char option [20];
			strcpy(option,p);
			if (strcmp(option, "CHARACTER") == 0)
			{
				p = strtok(NULL,"/");
				int ret = PreGameAssignChar(preGame,username,p);
				if(ret == 1)
				{
					//Missatge per comunicar al usari que acceptava
					printf("%s selecciona a %s\n", username, p);
					strcpy(response, "12/");
					strcat(response, "CHAROK");
					json_object* gameStateJson = GameStateToJson(preGame);
					strcpy(gameNotification, "10/");
					strcat(gameNotification, json_object_to_json_string_ext(gameStateJson, JSON_C_TO_STRING_PRETTY));
					gameSend = 1;
					gameSendUserState = 1;
					free(gameStateJson);
				}
				else
			    {
					//Missatge per comunicar al usari que acceptava
					printf("%s no pot seleccionar a %s\n", username, p);
					strcpy(response, "12/");
					strcat(response, "CHARFAIL");
				}
			}
			else if (strcmp(option, "START") == 0)
			{
				if(AllHasCharacter(preGame))
				{
					char notif2[SERVER_RSP_LEN];
					char notif3[SERVER_RSP_LEN];
					printf("Comença la partida %s\n", gameName);
					userSend = 0;
					gameSend = 0;
					strcpy(notif2, "12/START");
					sendToGame(gameSenderArgs[gameId], notif2, 1);
					/*for(int i=0;i<preGame->userCount;i++)
					{
						if(preGame->users[i]->userState == 1)
							write(preGame->users[i]->socket, notify_group, strlen(notify_group));
					}
					*/
					
					//sleep(1);
					pthread_mutex_lock(preGame->game_mutex);
					sprintf(notif3, "9/LOSE/%s/%s/",preGame->gameName,preGame->creator->username);
					pthread_mutex_unlock(preGame->game_mutex);
					sendToGame(gameSenderArgs[gameId], notif3, 0); 
					/*strcat(notify_group, "|");
					
					for(int i=0;i<preGame->userCount;i++)
					{
						if(preGame->users[i]->userState == 0)
							write(preGame->users[i]->socket, notify_group, strlen(notify_group));
					}
					
					pthread_mutex_unlock(preGame->game_mutex);
					*/
				}
				
				else
			    {
					printf("No ha triat tothom el seu personatge en %s\n", gameName);
					strcpy(response, "12/");
					strcat(response, "NOTALLSELECTED");
				}
			}
			else if (strcmp(option, "CANCEL") == 0)
			{
				userSend = 0;
				gameSend = 0;
				pthread_mutex_lock(preGame->game_mutex);
				sprintf(gameNotification, "12/CANCEL/%s/%s/",preGame->gameName,preGame->creator->username);
				sendToGame(gameSenderArgs[gameId], gameNotification, 0); 
				sendToGame(gameSenderArgs[gameId], gameNotification, 1); 
				DeleteGameFromTable(gameTable,preGame);
			}
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
		if((request_code)&&(userSend))
		{	
			strcat(response, "|");
			printf("%s = %s\n", threadArgs->connectedUser->username, response);
			write (sock_conn, response, strlen(response));	
		}
		
		if (gameSend)
		{
			sendToGame(gameSenderArgs[gameId], gameNotification, gameSendUserState);
		}
		
		// enviem notificacions globals
		if (globalSend)
		{
			sendToAll(threadArgs->senderArgs, globalResponse); 
		}
	}
	
	close(sock_conn);
	DelConnectedByName(connectedList, connectedUser->username);
	
	
	
	// modifiquem el punter a threadArgs per indicar thread disponible
	pthread_mutex_lock(threadArgs->threadArgs_mutex);
	threadArgs->connectedUser = NULL; //El punter de l'usuari esborrat ara val NULL	
	pthread_mutex_unlock(threadArgs->threadArgs_mutex);
	
	json_object* listJson = connectedListToJson(connectedList);
	
	// si modifiquem la llista, cal enviar la nova llista de forma global
	strcpy(globalResponse, "6/");
	strcat(globalResponse, json_object_to_json_string(listJson));
	sendToAll(threadArgs->senderArgs, globalResponse); 
	
	// COMPROVAR SI LA LLIBRERIA DISPOSA D'UN METODE PER ELIMINAR json_object
	free(listJson);
	
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
	// escucharemos en el puerto 50084, 50085 i/o 50086
	serv_addr.sin_port = htons(SHIVA_PORT);
	if (bind(sock_listen, (struct sockaddr *) &serv_addr, sizeof(serv_addr)) < 0)
		printf ("Error al bind");
	//La cola de requestes pendientes no podr? ser superior a 4
	if (listen(sock_listen, 2) < 0)
		printf("Error en el Listen");
	
	//CONNEXIÓ
	pthread_t threadConnected[CNCTD_LST_LENGTH];
	pthread_t senderThread;		// creem el thread del sender global
	//pthread_t gameSenderThread;
	//int sockets[CNCTD_LST_LENGTH];
	
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
	
	
	// inicialitzem el ThreadArgs que passarem al thread del sender
	ThreadArgs senderThreadArgs;
	pthread_mutex_t* mutexSenderThreadArgs = malloc(sizeof(pthread_mutex_t));
	pthread_mutex_init(mutexSenderThreadArgs, NULL);	
	senderThreadArgs.connectedList = connectedList;
	senderThreadArgs.gameTable = gameTable;
	senderThreadArgs.connectedUser = NULL;
	senderThreadArgs.threadArgs_mutex = mutexSenderThreadArgs;
	
	// incialitzem els SenderArgs (només n'hi ha d'haver una instància en tot el programa)
	SenderArgs senderArgs;
	pthread_cond_t* signalSender = malloc(sizeof(pthread_cond_t));
	pthread_cond_init(signalSender, NULL);
	pthread_mutex_t* mutexSender = malloc(sizeof(pthread_mutex_t));
	pthread_mutex_init(mutexSender, NULL);
	senderArgs.sender_mutex = mutexSender;		// mutex per bloquejar el sender thread
	senderArgs.sender_signal = signalSender;	// senyal per activar el sender thread
	
	// passem el senderArgs al ThreadArgs del sender
	senderThreadArgs.senderArgs = &senderArgs;
	
	// creem el thread del sender
	pthread_create(&senderThread, NULL, globalSender, &senderThreadArgs);
	
	// inicialitzem els threadArgs que passarem al thread per enviar a partides
	/*ThreadArgs gameSenderThreadArgs;
	pthread_mutex_t* mutexGameSenderThreadArgs = malloc(sizeof(pthread_mutex_t));
	pthread_mutex_init(mutexGameSenderThreadArgs, NULL);	
	gameSenderThreadArgs.connectedList = NULL;
	gameSenderThreadArgs.gameTable = gameTable;
	gameSenderThreadArgs.connectedUser = NULL;
	gameSenderThreadArgs.threadArgs_mutex = mutexSenderThreadArgs;
	gameSenderThreadArgs.senderArgs = NULL;
	
	pthread_create(&gameSenderThread, NULL, gameSender, &gameSenderThreadArgs);
	*/

	
	// inicialitzem els arguments dels senders de cada partida i arrenquem els threads
	pthread_t threadGameSender[MAX_GAMES];
	GameSenderArgs* gameSenderArgs[MAX_GAMES];
	for (int i = 0; i < MAX_GAMES; i++)
	{
		gameSenderArgs[i] = malloc(sizeof(GameSenderArgs));
	}
	//pthread_mutex_t* gameSenderMutex[MAX_GAMES];
	for(int i = 0; i < MAX_GAMES; i++)
	{
		gameSenderArgs[i]->game = NULL;
		//gameSenderArgs[i].gameResponse = NULL;
		gameSenderArgs[i]->gameSender_mutex = malloc(sizeof(pthread_mutex_t));
		pthread_mutex_init(gameSenderArgs[i]->gameSender_mutex, NULL);
		gameSenderArgs[i]->gameSender_signal = malloc(sizeof(pthread_cond_t));
		pthread_cond_init(gameSenderArgs[i]->gameSender_signal , NULL);
		gameSenderArgs[i]->userState = 0;
	}
	
	
	
	
	// passem a threadArgs els punters a la taula de partides
	// i la llista d'usuaris connectats, iguals per cada element.
	// assignem a cada element de l'array el mutex per accedir als arguments dels threads
	// També passem els senderArgs per a que puguin fer arribar respostes al sender global
	pthread_mutex_t* mutexThreadArgs = malloc(sizeof(pthread_mutex_t));
	pthread_mutex_init(mutexThreadArgs, NULL);
	ThreadArgs threadArgs[CNCTD_LST_LENGTH];
	for(int i = 0; i < CNCTD_LST_LENGTH; i++)
	{
		threadArgs[i].connectedList = connectedList;
		threadArgs[i].gameTable = gameTable;
		threadArgs[i].connectedUser = NULL;
		threadArgs[i].threadArgs_mutex = mutexThreadArgs;
		threadArgs[i].senderArgs = &senderArgs;
		threadArgs[i].gameSenderArgs = gameSenderArgs;
	}
	
	
	for (int i = 0; i < MAX_GAMES; i++)
	{
		pthread_create(&threadGameSender[i], NULL, gameSender, gameSenderArgs[i]);
	}
	
	// Atenem infinites peticions
	int freeSpace = 0;
	int i = 0;
	while(1)
	{
		printf ("Escuchando\n");					
		//sock_conn es el socket que usaremos para este cliente
		sock_conn = accept(sock_listen, NULL, NULL);				
		printf ("He recibido conexi?n\n");
		
		i = 0;
		freeSpace = 0;				
		pthread_mutex_lock(mutexThreadArgs);
		if (threadArgs[i].connectedList->number >= CNCTD_LST_LENGTH)
		{
			pthread_mutex_unlock(mutexThreadArgs);
			printf ("Llista jugadors plena\n");
			char response[20] = "FULL|";
			write (sock_conn, response, strlen(response));	
			printf("%s\n", response);
			close(sock_conn);
		}
		else
		{
			while ((i < CNCTD_LST_LENGTH) && (freeSpace == 0))
			{
				if (threadArgs[i].connectedUser == NULL)
				{
					freeSpace = 1;
					threadArgs[i].connectedUser = NULL;			
				}
				else
					i++;
			}
			pthread_mutex_unlock(mutexThreadArgs);
			if(freeSpace == 0)
			{
				printf("No hi ha espai a threadArgs");
				char response[20] = "FULL|";
				write (sock_conn, response, strlen(response));	
				printf("%s\n", response);
				close(sock_conn);
			}
			else
			{
				// avisem al client que estem preparats per atendre peticions
				char response[20] = "OK|";
				write (sock_conn, response, strlen(response));	
				printf("%s\n", response);
				
				// guardem l'id del socket
				// TODO: mirar si això es pot fer sense mutex, és a dir,
				// si modificar un int que forma part d'un array és atomic en C
				//senderArgs.sockets[i] = sock_conn;
				
				pthread_mutex_lock(mutexThreadArgs);
				// Creem el l'usuari per cada thread que s'instancia i el posem
				// a l'element de l'array threadArgs que es passa al thread
				threadArgs[i].connectedUser = CreateConnectedUser();
				threadArgs[i].connectedUser->socket = sock_conn;
				
				// creem el thread
				pthread_create(&threadConnected[i], NULL, attendClient, &threadArgs[i]);
				pthread_mutex_unlock(mutexThreadArgs);
				printf("Iterator: %d\n", i);
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
	pthread_mutex_destroy(mutexThreadArgs);
	pthread_mutex_destroy(mutexSender);
}
//------------------------------------------------------------------------------



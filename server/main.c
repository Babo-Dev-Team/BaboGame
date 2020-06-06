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
#include <unistd.h>

#include "BBDD_Handler.h"
#include "globals.h"
#include "connected_list.h"
#include "game_table.h"
#include "game_state.h"

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
	int newData;
}GameSenderArgs;

// el client passa les updates amb una instancia d'aquesta estructura
typedef struct GameUpdatesFromClient{
	CharacterState* charState;
	//ProjectileState** projectileStates;
	int userId;
	int newDataFromClient;
	int backOffRequested;
	//int projectileCount;
}GameUpdatesFromClient;

// el thread de process necessita el seu sender de partida, aixi com un array de punters a 
// totes les estructures dels clients de la partida per a rebre les updates
typedef struct GameProcessorArgs{
	GameSenderArgs* senderArgs;
	GameUpdatesFromClient** gameUpdatesFromClients;
	int initEnabled;
	int processEnabled;
	int n_players;
	int gameId;
	pthread_mutex_t* gameProcessor_mutex;
	pthread_cond_t* gameProcessor_signal;
}GameProcessorArgs;

// Arugments que es passen a cada thread
typedef struct ThreadArgs{
	ConnectedList* connectedList;			// punter a la llista de connectats
	ConnectedUser* connectedUser;			// punter a l'usuari que gestiona el thread 
	GameTable* gameTable;					// punter a la taula de partides	
	pthread_mutex_t* threadArgs_mutex;	
	SenderArgs* senderArgs;
	GameSenderArgs** gameSenderArgs;
	GameProcessorArgs** gameProcessorArgs;
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

int** GetInitialPositions (int n_players, int game_max_x, int game_max_y)
{
	int** positions = malloc(n_players * sizeof(int*));
	for (int i = 0; i < n_players; i++)
	{
		positions[i] = malloc(2 * sizeof(int));
		positions[i][0] = rand() % game_max_x;
		positions[i][1] = rand() % game_max_y;
	}
	return positions;
}

// funció per enviar a tothom, activa el thread globalSender
void sendToGame (GameSenderArgs* args, char response[SERVER_RSP_LEN], int userState)
{
	struct timespec s;
	s.tv_sec = 0;
	s.tv_nsec = 1000000L;
	if (args->newData != 0)
	{
		nanosleep(&s, NULL);
	}
	
	// adquirim el lock del sender (alliberat pel pthread_mutex_wait en el seu thread)
	pthread_mutex_lock(args->gameSender_mutex);
	int waitNotified = 0;
	int unlocked = 0;
	
	// si el gameSender no ha processat les dades anteiors però ens deixa
	// adquirie el lock, és perquè encara no ha sortit del pthread_condition_wait.
	// el que fem és anar alliberant el mutex i esperar que el sender adquireixi el mutex
	// i envii la informació. podem llegir userState perquè és un int32 (atomic)
	/*while (args->newData != 0)
	{
		pthread_mutex_unlock(args->gameSender_mutex);
		unlocked = 1;
		if(!waitNotified)
		{
			printf("sendToGame: waiting for game sender to wake up...\n");
			waitNotified = 1;
		}
	}
	if (unlocked)
	{
		pthread_mutex_lock(args->gameSender_mutex);
	}*/
	args->newData = 1;
	args->userState = userState;
	pthread_cond_signal(args->gameSender_signal);
	strcpy(args->gameResponse, response);

	// ara indiquem al thread que s'executi
	pthread_mutex_unlock(args->gameSender_mutex);
}

void* gameProcessor (void* args)
{
	GameProcessorArgs* procArgs = (GameProcessorArgs*) args;
	GameSenderArgs* senderArgs = procArgs->senderArgs;
	GameUpdatesFromClient** clientUpdates;
	
	int n_clientsInit = 0;
	pthread_mutex_lock(procArgs->gameProcessor_mutex);
	while (!procArgs->initEnabled)
	{	
		printf("Game Processor entering sleep...\n");
		pthread_cond_wait(procArgs->gameProcessor_signal, procArgs->gameProcessor_mutex);
	}
	//pthread_mutex_lock(procArgs->gameProcessor_mutex);
	printf("Game Processor performing init...\n");
	//pthread_mutex_lock(procArgs->gameProcessor_mutex);
	int n_players = procArgs->n_players;
	int gameId = procArgs->gameId;
	char response[SERVER_RSP_LEN];
	GameState* gameState = CreateGameState(gameId, n_players);
	int** gamePositions = GetInitialPositions(n_players, SCREEN_MAX_X, SCREEN_MAX_Y);
	SetInitialPositions(gameState, gamePositions);
	
	
	
	//pthread_mutex_unlock(procArgs->gameProcessor_mutex);
	
	while(!procArgs->processEnabled)
	{
		//pthread_mutex_lock(procArgs->gameProcessor_mutex);
		printf("Game processor sending init state\n");
		UpdateGameStateJson(gameState);
		sprintf(response, "103/%s", json_object_to_json_string_ext(gameState->gameStateJson, JSON_C_TO_STRING_PRETTY));
		sendToGame(senderArgs, response, 1);		
		
		// auto enable realtime processing when all clients have been inisialized
		if(++n_clientsInit >= procArgs->n_players)
		{
			printf("All Clients Initialized!\n");
			procArgs->processEnabled = 1;
			pthread_mutex_unlock(procArgs->gameProcessor_mutex);
			sleep(1);
		}
		else
		{
			procArgs->initEnabled = 0;
			while(!procArgs->initEnabled)
			{			
				printf("Game Processor entering sleep...\n");
				pthread_cond_wait(procArgs->gameProcessor_signal, procArgs->gameProcessor_mutex);		
			}
		}
	}
	printf("Game Processor starting realtime processing...\n");

	sendToGame(senderArgs, "102/START", 1);
	
	clientUpdates = procArgs->gameUpdatesFromClients; 
	for (int i = 0; i < procArgs->n_players; i++)
	{
		clientUpdates[i]->charState->characterId = i;
	}
	

	int counter = 0;
	int operation = 1;
	
	gameState->playable = 1;
	
	// this function will auto-disable when game is over
	while(procArgs->processEnabled)
	{
		/*pthread_mutex_lock(senderArgs->game->game_mutex);
		for (int i = 0; i < n_players; i++)
		{
			int backOff = clientUpdates[i]->backOffRequested; 
			// comprovem que estem accedint al user que toca!
			// i restringim les updates a aquest user si ha demanat backOff
			if(senderArgs->game->users[i]->charId == clientUpdates[i]->charState->characterId)
			{
				if(backOff)
				{
					senderArgs->game->users[i]->userState = 3;
				}
				else senderArgs->game->users[i]->userState = 1;
			}
			else 
			{
				printf("CRITICAL ERROR: Char ID's DO NOT MATCH\n");
				pthread_exit(103);
			}
		}*/
		//gameState->projectileCount = 0;
		
		
		
		for (int i = 0; i < n_players; i++)
		{
			if (clientUpdates[i]->newDataFromClient)
			{
				int userId = clientUpdates[i]->userId;
				int charId = clientUpdates[i]->charState->characterId;
				gameState->characterStatesList[charId].position_X = clientUpdates[i]->charState->position_X;
				gameState->characterStatesList[charId].position_Y = clientUpdates[i]->charState->position_Y;
				gameState->characterStatesList[charId].velocity_X = clientUpdates[i]->charState->velocity_X;
				gameState->characterStatesList[charId].velocity_Y = clientUpdates[i]->charState->velocity_Y;
				gameState->characterStatesList[charId].direction_X = clientUpdates[i]->charState->direction_X;
				gameState->characterStatesList[charId].direction_Y = clientUpdates[i]->charState->direction_Y;
				gameState->characterStatesList[charId].health = clientUpdates[i]->charState->health;
				
				int n_proj = clientUpdates[i]->charState->projectileCount;
				gameState->characterStatesList[charId].projectileCount = n_proj;
				for (int j = 0; j < n_proj; j++)
				{
					gameState->characterStatesList[charId].projectileStates[j].projectileID = clientUpdates[i]->charState->projectileStates[j].projectileID;
					gameState->characterStatesList[charId].projectileStates[j].shooterID = clientUpdates[i]->charState->projectileStates[j].shooterID;
					gameState->characterStatesList[charId].projectileStates[j].projectileType = clientUpdates[i]->charState->projectileStates[j].projectileType;
					gameState->characterStatesList[charId].projectileStates[j].position_X = clientUpdates[i]->charState->projectileStates[j].position_X;
					gameState->characterStatesList[charId].projectileStates[j].position_Y = clientUpdates[i]->charState->projectileStates[j].position_Y;
					gameState->characterStatesList[charId].projectileStates[j].direction_X = clientUpdates[i]->charState->projectileStates[j].direction_X;
					gameState->characterStatesList[charId].projectileStates[j].direction_Y = clientUpdates[i]->charState->projectileStates[j].direction_Y;
					gameState->characterStatesList[charId].projectileStates[j].LinearVelocity = clientUpdates[i]->charState->projectileStates[j].LinearVelocity;
					gameState->characterStatesList[charId].projectileStates[j].hitCount = clientUpdates[i]->charState->projectileStates[j].hitCount;
				}
				
				clientUpdates[i]->newDataFromClient = 0;
			}
		}

/*
		if (operation)
		{
			gameState->characterStatesList[0].position_X += 2;
			gameState->characterStatesList[0].position_Y += 2;
			gameState->characterStatesList[1].position_X -= 2;
			gameState->characterStatesList[1].position_Y -= 2;
			
			
		}
		else 
		{
			gameState->characterStatesList[0].position_X -= 2;
			gameState->characterStatesList[0].position_Y -= 2;
			gameState->characterStatesList[1].position_X += 2;
			gameState->characterStatesList[1].position_Y += 2;
		}
		
		counter++;
		if (counter > 150)
		{
			counter = 0;
			if (operation)
			{
				operation = 0;
			}
			else operation = 1;
		}
		*/
		UpdateGameStateJson(gameState);
		sprintf(response, "103/%s", json_object_to_json_string_ext(gameState->gameStateJson, JSON_C_TO_STRING_PRETTY));
		sendToGame(senderArgs, response, 1);
		
		// sleep for 15 ms
		struct timespec s;
		s.tv_sec = 0;
		s.tv_nsec = 10000000L;
		nanosleep(&s, NULL);
		
		// calcular quan acaba la partida
		
	}
	sendToGame(senderArgs, "102/END", 1);
	pthread_exit(0);
}



void* gameSender (void* args)
{
	GameSenderArgs* senderArgs = (GameSenderArgs*) args;
	//char gameResponse [SERVER_RSP_LEN];
	
	GameSenderArgs thisSenderArgs;
	
	// inicialitzem un array de sockets actius
	int activeSocketList[MAX_GAME_USRCOUNT];
	int backOffTimers[MAX_GAME_USRCOUNT];
	int backOffRequested[MAX_GAME_USRCOUNT];
	for (int i = 0; i < MAX_GAME_USRCOUNT; i++)
	{
		backOffRequested[i] = 0;
		backOffTimers[i] = 0;
	}
	int userStates[MAX_GAME_USRCOUNT];
	for (int i = 0; i < MAX_GAME_USRCOUNT; i++)
	{
		activeSocketList[i] = -1;
		userStates[i] = -2;
	}
	// hem de bloquejar els senderArgs per a que 
	// pthread_condition_wait s'executi correctament
	pthread_mutex_lock(senderArgs->gameSender_mutex);
	
	while(1)
	{
		
		// indiquem posant userState a -2 que estem esperant per rebre noves dades.
		// No farem res fins que sendToGame no ens passi info i un userState valid per enviar
		senderArgs->newData = 0;
		while(senderArgs->newData == 0)
		{
		//	printf("game sender entering sleep\n");
			// alliberem els args per a que la funcio sendToAll els pugui modificar
			// i esperem que ens indiqui amb el sender_signal que tenim dades per enviar
			if(pthread_cond_wait(senderArgs->gameSender_signal, senderArgs->gameSender_mutex))
			{
				printf("game sender CONDITION WAIT ERROR\n");
			}
		}
		// consultem els sockets actius a la llista de connectats	
		
		// la idea es que durant la partida, el thread de calcul cridi directament la funcio sendtogame.
		//printf("game sender waking up\n");
		thisSenderArgs.userState = senderArgs->userState;
		thisSenderArgs.game = senderArgs->game;
		strcpy(thisSenderArgs.gameResponse, senderArgs->gameResponse);
		//pthread_mutex_unlock(senderArgs->gameSender_mutex);
		
		
		pthread_mutex_lock(thisSenderArgs.game->game_mutex);
		int n_users = thisSenderArgs.game->userCount;
		for(int i = 0; i < n_users; i++)
		{
			if(backOffRequested[i] == 1 && backOffTimers[i] == 0)
			{
				backOffRequested[i] = 0;
				thisSenderArgs.game->users[i]->userState = 1;
				printf("Timed back-off: resume realtime notifications\n");
			}
			else if(thisSenderArgs.game->users[i]->userState == 3 && backOffRequested[i] == 0)
			{
				backOffRequested[i] = 1;
				backOffTimers[i] = BACKOFF_TICKS;
			}
			else if (backOffRequested[i] == 1 && backOffTimers[i] > 0)
			{
				--backOffTimers[i];
			}
		}
		for(int i = 0; i < n_users; i++)
		{
			activeSocketList[i] = thisSenderArgs.game->users[i]->socket;
			userStates[i] = thisSenderArgs.game->users[i]->userState;
		}
	
		pthread_mutex_unlock(thisSenderArgs.game->game_mutex);
		
		
		
		strcat(thisSenderArgs.gameResponse, "|");
		
		// enviem a tots els sockets actius
		//printf("Sending to game users with User State = %d\n", thisSenderArgs.userState);
		for (int i = 0; i < n_users; i++)
		{		
			if (thisSenderArgs.userState == userStates[i])
			{
				//printf ("GAME ID %d NOTIFICATION for %s = %s\n", thisSenderArgs.game->gameId, thisSenderArgs.game->users[i]->username, thisSenderArgs.gameResponse);
				write (activeSocketList[i], thisSenderArgs.gameResponse, strlen(thisSenderArgs.gameResponse));			
			}
		}	
		//pthread_mutex_unlock(senderArgs->gameSender_mutex);
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
	GameProcessorArgs** gameProcessorArgs = threadArgs->gameProcessorArgs;

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
	
	int userId;
	int charId = -1;
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
		request[0] = '\0';
		request_string[0] = '\0';
		
		// Ahora recibimos el mensaje, que dejamos en request
		request_length = read(sock_conn, request, sizeof(request));
		if (request_length >= CLIENT_REQ_LEN)
		{
			printf("Error: socket data exceeds maximum request size. Discarding socket data...\n");
			char discardStr[524288];
			read(sock_conn, discardStr, sizeof(discardStr));
		}
		else if (request_length == -1)
		{
			printf ("Socket Read Error\n");
			disconnect = 1;
		}
		else if (request_length == 0)
		{
			printf ("Request is empty!!\n");
			disconnect = 1;
		}
		else
		{
			//printf ("Socket read: received request\n");
			// marcamos el final de string
			request[request_length]='\0';
			strcpy(request_string, request);
			//printf("Full request: %s\n", request_string);
			char *p = strtok(request, "/");
			int request_code =  atoi(p); // sacamos el request_code del request
			//printf("Request Code: %d\n", request_code);
			
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
					userId = id;
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
					userId = id;
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
				char message [CLIENT_REQ_LEN];
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
				//printf("%s\n",request_string);
				
				p = strtok(NULL,"/");
				char option [CLIENT_REQ_LEN];
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
						
						//json_object* initState = GameInitStateJson(preGame, preGameUser->id);
						//printf("\n\n%s\n\n", json_object_to_json_string_ext(initState, JSON_C_TO_STRING_PRETTY));
						
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
						pthread_mutex_lock(gameProcessorArgs[gameId]->gameProcessor_mutex);
						gameProcessorArgs[gameId]->gameId = preGame->gameId;
						gameProcessorArgs[gameId]->n_players = preGame->userCount;
						
						// incialitzem les estructures per a que els clients reportin els updates
						gameProcessorArgs[gameId]->gameUpdatesFromClients = malloc(preGame->userCount * sizeof(GameUpdatesFromClient*));
						for (int i = 0; i < preGame->userCount; i++)
						{
							gameProcessorArgs[gameId]->gameUpdatesFromClients[i] = malloc(sizeof(GameUpdatesFromClient));
							gameProcessorArgs[gameId]->gameUpdatesFromClients[i]->newDataFromClient = 0;
							gameProcessorArgs[gameId]->gameUpdatesFromClients[i]->backOffRequested = 0;
							//gameProcessorArgs[gameId]->gameUpdatesFromClients[i]->projectileCount = 0;
							
							gameProcessorArgs[gameId]->gameUpdatesFromClients[i]->userId = preGame->users[i]->id;
							gameProcessorArgs[gameId]->gameUpdatesFromClients[i]->charState = malloc(sizeof(CharacterState));
							gameProcessorArgs[gameId]->gameUpdatesFromClients[i]->charState->projectileCount = 0;
							
							//gameProcessorArgs[gameId]->gameUpdatesFromClients[i]->projectileStates = malloc(PROJ_COUNT_PLAYER * sizeof(ProjectileState*));
							/*
							for (int j = 0; j < PROJ_COUNT_PLAYER; j++)
							{
								gameProcessorArgs[gameId]->gameUpdatesFromClients[i]->projectileStates[j] = malloc(sizeof(ProjectileState));
							}*/
																											
							//gameProcessorArgs[gameId]->gameUpdatesFromClients[i]->charState->characterId = i;
							//gameProcessorArgs[gameId]->gameUpdatesFromClients[i]->charState->position_X = 0;
							//gameProcessorArgs[gameId]->gameUpdatesFromClients[i]->charState->position_Y = 0;
							//gameProcessorArgs[gameId]->gameUpdatesFromClients[i]->charState->velocity_X = 0;
							//gameProcessorArgs[gameId]->gameUpdatesFromClients[i]->charState->velocity_Y = 0;			
						}
						pthread_mutex_unlock(gameProcessorArgs[gameId]->gameProcessor_mutex);
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
			
			case 101:
			{
				//printf("Request 101: %s\n", request);
				userSend = 0;
				charId = GetCharIdFromUserId(preGame, userId);
				printf("char ID assigned to user %s: %d\n", username, charId);
				p = strtok(NULL, "/");
				if(strcmp(p, "HELLO") == 0)
				{
					char initStateResponse [SERVER_RSP_LEN];
					json_object* initJson = GameInitStateJson(preGame, connectedUser->id);
					if (initJson != NULL)
					{
						sprintf(initStateResponse, "101/%s|", json_object_to_json_string_ext(initJson, JSON_C_TO_STRING_PRETTY));
						write(sock_conn, initStateResponse, strlen(initStateResponse));
						printf("Sending initial state to user %s:\n%s", connectedUser->username, initStateResponse);
					}
					else 
					{
						printf("JSON ERROR: OBJECT IS NULL\n");
					}
					
					
					// wake up the processor to allow init and send first game state to all clients
					sleep(1);
					gameProcessorArgs[gameId]->initEnabled = 1;
					pthread_cond_signal(gameProcessorArgs[gameId]->gameProcessor_signal);				
					
				}
				break;				
			}
			case 103:
			{
				userSend = 0;
				gameSend = 0;
				globalSend = 0;
				p = strtok(NULL, "/");
				if(!strcmp(p, "BACK-OFF"))
				{
					printf("user %s is requesting server back-off\n", username);
					if (charId != -1)
					{
						printf("user has a valid char ID, access to pre game user by array index is possible\n");
						//gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->backOffRequested = 1;
						pthread_mutex_lock(preGame->game_mutex);
						
						if(preGame->users[charId]->charId == charId)
						{
							preGame->users[charId]->userState = 3;
						}
						else
						{
							printf("CRITICAL ERROR: Char ID's DO NOT MATCH\n");
							int* error = malloc(sizeof(int));
							*error = 103;
							pthread_exit(error);
						}
						pthread_mutex_unlock(preGame->game_mutex);
						printf("Back Off granted\n");
						
					}
					else 
					{
						printf("No Char ID has been established yet, cannot access pre game users by array index\n");
					}
					
				}
				else if(!strcmp(p, "RESUME"))
				{
					printf("user %s is requesting server resume\n", username);
					if (charId != -1)
					{
						printf("user has a valid char ID, access to pre game user by array index is possible\n");
						pthread_mutex_lock(preGame->game_mutex);
						if(preGame->users[charId]->charId == charId)
						{
							preGame->users[charId]->userState = 1;
						}
						else
						{
							printf("CRITICAL ERROR: Char ID's DO NOT MATCH\n");
							int* error = malloc(sizeof(int));
							*error = 103;
							pthread_exit(error);
						}
						pthread_mutex_unlock(preGame->game_mutex);
						printf("Resume granted\n");	
					}
					else 
					{
						printf("No Char ID has been established yet, cannot access pre game users by array index\n");
					}
				}
				break;
			}
			case 104: //Actualització estat del client
			{
				userSend = 0;
				globalSend = 0;
				gameSend = 0;
				
				/*{
					"characterState":
					{
						"charID":0,
						"posX":125,
						"posY":80,
						"velX":0,
						"velY":0
					},					
					"projectileStates":[]
				}*/
				
				p = strtok(NULL, "|");
				if(p == NULL)
				{
					printf("Warning: 104 request extracted as Null! (strtok returned null)\n");
				}
				else
				{
					//printf("Debug: p = %s\n", p);
					
					json_object* obj = json_tokener_parse(p);
					
					json_object* jsonCharState;
					json_object_object_get_ex(obj, "characterState", &jsonCharState);
					
					json_object* jsonCharId;
					json_object* posX;
					json_object* posY;
					json_object* velX;
					json_object* velY;
					json_object* dirX;
					json_object* dirY;
					json_object* health;
					json_object_object_get_ex(jsonCharState, "charID", &jsonCharId);
					json_object_object_get_ex(jsonCharState, "posX", &posX);
					json_object_object_get_ex(jsonCharState, "posY", &posY);
					json_object_object_get_ex(jsonCharState, "velX", &velX);
					json_object_object_get_ex(jsonCharState, "velY", &velY);
					json_object_object_get_ex(jsonCharState, "dirX", &dirX);
					json_object_object_get_ex(jsonCharState, "dirY", &dirY);
					json_object_object_get_ex(jsonCharState, "health", &health);
					
					int receivedCharId = json_object_get_int(jsonCharId);
					int pX = json_object_get_int(posX);
					int pY = json_object_get_int(posY);
					int vX = json_object_get_int(velX);
					int vY = json_object_get_int(velY);
					double dX = json_object_get_double(dirX);
					double dY = json_object_get_double(dirY);
					int h = json_object_get_int(health);
					
					//LLista de projectils I don't know da wae xd
					json_object* jsonProjectileListState;
					json_object_object_get_ex(obj, "projectileStates", &jsonProjectileListState);
					
					if(!json_object_is_type(jsonProjectileListState, json_type_array))
					{
						printf("Warning: received invalid projectile array syntax. ignoring message.\n");
					}
					else 
					{
						int arrayLen = json_object_array_length(jsonProjectileListState);
						//printf("Debug: Projectile states has %d elements\n", arrayLen);
						
						json_object* jsonProjState;
						json_object* projectileId;
						json_object* shooterId;
						json_object* projectileType;
						json_object* projectilePosX;
						json_object* projectilePosY;
						json_object* projectileDirX;
						json_object* projectileDirY;
						json_object* projectileLinearVelocity;
						json_object* hitCount;
						
						/*	int projId;
						int shtrId;
						char projType;
						int projPx;
						int projPy;
						double projDx;
						double projDy;
						double projLinVel;
						int n_hits;
						*/	
						ProjectileState projState[arrayLen];
						
						for (int i = 0; i < arrayLen; i++) 
						{
							jsonProjState = json_object_array_get_idx(jsonProjectileListState, i);
							
							projectileId = json_object_object_get(jsonProjState, "projectileID");
							shooterId = json_object_object_get(jsonProjState, "shooterID");
							projectileType = json_object_object_get(jsonProjState, "projectileType");
							projectilePosX = json_object_object_get(jsonProjState, "posX");
							projectilePosY = json_object_object_get(jsonProjState, "posY");
							projectileDirX = json_object_object_get(jsonProjState, "directionX");
							projectileDirY = json_object_object_get(jsonProjState, "directionY");
							projectileLinearVelocity = json_object_object_get(jsonProjState, "LinearVelocity");
							hitCount = json_object_object_get(jsonProjState, "hitCount");
							
							projState[i].projectileID = json_object_get_int(projectileId);
							projState[i].shooterID = json_object_get_int(shooterId);
							projState[i].projectileType = *(json_object_get_string(projectileType));
							projState[i].position_X = json_object_get_int(projectilePosX);
							projState[i].position_Y = json_object_get_int(projectilePosY);
							projState[i].direction_X = json_object_get_int(projectileDirX);
							projState[i].direction_Y = json_object_get_int(projectileDirY);
							projState[i].LinearVelocity = json_object_get_int(projectileLinearVelocity);
							projState[i].hitCount = json_object_get_int(hitCount);
							
							/*printf("Debug: Data from proj %d: ID: %d, shooter ID: %d, Type: %c\nPosX: %d, PosY: %d DirX: %f, DirY: %f, Vel: %f, hitCount: %d\n",
							i, projState[i].projectileID, projState[i].shooterID, projState[i].projectileType, projState[i].position_X, projState[i].position_Y,
							projState[i].direction_X, projState[i].direction_Y, projState[i].LinearVelocity, projState[i].hitCount);
							*/
							
						}
						
						
						//printf("Received: ChardId: %d, PosX: %d, PosY: %d, VelX: %d, VelY: %d\n", receivedCharId, pX, pY, vX, vY);
						
						if (receivedCharId != charId)
						{
							printf("Warning: Received char Id does not match the handled char Id. Ignoring message.\n");
						}
						else 
						{
							if (!gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->newDataFromClient)
							{
								//printf("Debug: can write to GameUpdatesFromClient\n");
								gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->position_X = pX;
								gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->position_Y = pY;
								gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->velocity_X = vX;
								gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->velocity_Y = vY;
								gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->direction_X = dX;
								gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->direction_Y = dY;
								gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->health = h;
								
								if (arrayLen > PROJ_COUNT_PLAYER)
								{
									printf("Warning: projectile json array has too many elements! ignoring some projectiles\n");
									arrayLen = PROJ_COUNT_PLAYER;
								}
								
								gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->projectileCount = arrayLen;
								for (int i = 0; i < arrayLen; i++) 
								{
									gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->projectileStates[i].projectileID = projState[i].projectileID;
									gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->projectileStates[i].shooterID = projState[i].shooterID;
									gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->projectileStates[i].projectileType = projState[i].projectileType;
									gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->projectileStates[i].position_X = projState[i].position_X;
									gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->projectileStates[i].position_Y = projState[i].position_Y;
									gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->projectileStates[i].direction_X = projState[i].direction_X;
									gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->projectileStates[i].direction_Y = projState[i].direction_Y;
									gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->projectileStates[i].LinearVelocity = projState[i].LinearVelocity;
									gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->charState->projectileStates[i].hitCount = projState[i].hitCount;
								}
								
								gameProcessorArgs[gameId]->gameUpdatesFromClients[charId]->newDataFromClient = 1;							
							}
							else 
							{
								//printf("Debug: cannot write to GameUpdatesFromClient\n");
							}					
						}
					}
				}
				break;
			}
			
			// request 0 -> Disconnect	
			// TODO: S'ha d'eliminar el user de la llista de connectats!!!
			case 0:
			{
				
				userSend = 0;
				char request_str[CLIENT_REQ_LEN];
				p = strtok(NULL, "/");
				if (p == NULL)
				{
					printf("Warning: received invalid disconnect request (strtok returned null)\n");
				}
				else
				{
					strcpy(request_str, strtok(NULL, "/"));
					if(!strcmp(request_str, "DISCONNECT"))
					{
						// Se acabo el servicio para este cliente
						disconnect = 1;
						printf("Received disconnect notification\n");
					}
					else printf("Warning: request beginning by 0/ not valid\n");
					break;	
				}
			}
				
			default:
				printf("Warning: request_code no reconocido.\n");
				userSend = 0;
				globalSend = 0;
				gameSend = 0;
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
	/*int **positions;
	positions = GetInitialPositions(4, SCREEN_MAX_X, SCREEN_MAX_Y);
	for (int i = 0; i < 4; i++)
	{
		printf("%d\t%d\n", positions[i][0], positions[i][1]);
	}*/
	
	/*GameState* state = CreateGameState(10, 4);
	char teststr [8192];
	strcpy(teststr, json_object_to_json_string_ext(state->gameStateJson, JSON_C_TO_STRING_PRETTY));	
	printf("\n\n%s\n\n", teststr);
	
	state->playable = 1;
	state->characterStatesList[0].position_X = 300;
	state->characterStatesList[0].position_Y = 200;
	state->characterStatesList[3].position_X = 30;
	state->characterStatesList[3].position_Y = 500;
	
	UpdateGameStateJson(state);
	strcpy(teststr, json_object_to_json_string_ext(state->gameStateJson, JSON_C_TO_STRING_PRETTY));	
	printf("\n\n%s\n\n", teststr);*/
	
	
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
		printf ("Socket Bind Error\n");
	//La cola de requestes pendientes no podr? ser superior a 4
	if (listen(sock_listen, 200) < 0)
		printf("Socket Listen Error\n");
	
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
		gameSenderArgs[i]->newData = 0;
		//gameSenderArgs[i].gameResponse = NULL;
		gameSenderArgs[i]->gameSender_mutex = malloc(sizeof(pthread_mutex_t));
		pthread_mutex_init(gameSenderArgs[i]->gameSender_mutex, NULL);
		gameSenderArgs[i]->gameSender_signal = malloc(sizeof(pthread_cond_t));
		pthread_cond_init(gameSenderArgs[i]->gameSender_signal , NULL);
		gameSenderArgs[i]->userState = 0;
	}
	
	// creem els threads i els args per als processors
	pthread_t threadGameProcessor[MAX_GAMES];
	GameProcessorArgs* gameProcessorArgs[MAX_GAMES];
	for (int i = 0; i < MAX_GAMES; i++)
	{
		gameProcessorArgs[i] = malloc(sizeof(GameProcessorArgs));
	}
	
	for(int i = 0; i < MAX_GAMES; i++)
	{
		gameProcessorArgs[i]->gameProcessor_mutex = malloc(sizeof(pthread_mutex_t));
		pthread_mutex_init(gameProcessorArgs[i]->gameProcessor_mutex, NULL);
		gameProcessorArgs[i]->gameProcessor_signal = malloc(sizeof(pthread_cond_t));
		pthread_cond_init(gameProcessorArgs[i]->gameProcessor_signal, NULL);
		gameProcessorArgs[i]->initEnabled = 0;
		gameProcessorArgs[i]->processEnabled = 0;
		
		// aqui es on estem assignant a cada processor el sender de la partida corresponent
		gameProcessorArgs[i]->senderArgs = gameSenderArgs[i];
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
		threadArgs[i].gameProcessorArgs = gameProcessorArgs;
	}
	
	
	for (int i = 0; i < MAX_GAMES; i++)
	{
		pthread_create(&threadGameSender[i], NULL, gameSender, gameSenderArgs[i]);
	}
	for (int i = 0; i < MAX_GAMES; i++)
	{
		pthread_create(&threadGameProcessor[i], NULL, gameProcessor, gameProcessorArgs[i]);
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



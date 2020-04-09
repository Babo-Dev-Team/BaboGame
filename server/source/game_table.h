#ifndef GAME_TABLE_H
#define GAME_TABLE_H

#include "globals.h"
#include "connected_list.h"

#define TOKEN_LEN 32

//------------------------------------------------------------------------------
// DATA STRUCTURES
//------------------------------------------------------------------------------

// usuari dins la pre-partida
typedef struct PreGameUser{
	int id;									// ID usuari
	int socket;								// socket associat a l'usuari
	char username[USRN_LENGTH];				// nom d'usuati
	char charname[CHAR_LEN];				// personatge seleccionat per la partida
	pthread_mutex_t* user_mutex;			// mutex de l'usuari
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
	//int softKill;					    	// bool per indicar partida acabada i pot ser eliminada
	pthread_mutex_t* game_mutex;			// mutex de la pre-partida
}PreGameState;

// taula de partides (informativa),
// és una taula de PreGameState
typedef struct GameTable{
	PreGameState* createdGames[MAX_GAMES];	// array de PreGameState
	int gameCount;							// nombre de partides
	pthread_mutex_t* game_table_mutex;		// mutex de la taula
}GameTable;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// PUBLIC METHODS
//------------------------------------------------------------------------------

int GameNameAvailable(GameTable* table, char name[GAME_LEN]);
PreGameUser* CreatePreGameUser(ConnectedUser* connectedUser);
int AddPreGameUser(PreGameState* gameState, PreGameUser* user);
int DeletePreGameUser(PreGameState* gameState, PreGameUser* user);
PreGameState* CreateGame(ConnectedUser* user, char name[GAME_LEN]);
int AddGameToGameTable(GameTable* table, PreGameState* gameState);
int DeleteGameFromTable(GameTable* table, PreGameState* gameState);
GameTable* CreateGameTable(pthread_mutex_t* mutex);
void DeleteGameTable(GameTable* table);
int PreGameAssignChar(PreGameState* gameState, char username[USRN_LENGTH], char charname[CHAR_LEN]);
json_object* GameTableToJson(GameTable* table);
//------------------------------------------------------------------------------

#endif

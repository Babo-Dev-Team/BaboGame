//------------------------------------------------------------------------------

// Funcions per crear, eliminar i gestionar la taula de partides, l'estat de les
// partides i els usuaris que hi participen durant la fase de creaci� i
// configuraci� de partida, i per a tenir una taula de partides actualitzada.

//------------------------------------------------------------------------------
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

#include "game_table.h"
#include "connected_list.h"

//------------------------------------------------------------------------------
// PRIVATE METHODS, NOT THREAD SAFE
//------------------------------------------------------------------------------

// m�tode per eliminar un usuari (privat)
void __deletePreGameUser(PreGameUser* user)
{
	free(user);
}

// m�tode per eliminar una partida (privat)
void __deletePreGame(PreGameState* game)
{
	for (int i = 0; i < game->userCount; i++)
	{
		__deletePreGameUser(game->users[i]);
	}
	//pthread_mutex_destroy(game->game_mutex);
	free(game);
}

// genera una seq��ncia aleat�ria que utilitzem com a token d'acc�s a la partida
// i la diposita en la string token
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

// No es thread safe! per us local de les funcions que gestionen la taula de partides
// i que ja fan servir lock
int __gameNameAvailable(GameTable* table, char name[GAME_LEN])
{
	int available = 1;
	int i = 0;
	while (i < MAX_GAMES && available)
	{
		if(table->createdGames[i] != NULL) 
		{
			if(!strcmp(table->createdGames[i]->gameName, name))
			{
				available = 0;
			}
		}
		++i;		
	}
	return available;
}

// afegim un PreGameUser a la llista d'usuaris de la partida
int __addPreGameUser(PreGameState* gameState, PreGameUser* user)
{
	if(gameState->userCount < MAX_GAME_USRCOUNT)
	{
		int pos = gameState->userCount;
		user->charId = pos;
		gameState->users[pos] = user;
		gameState->userCount++;		
		return 0;
	}
	else
	{
		return -1;
	}
}
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// PUBLIC METHODS, THREAD SAFE
//------------------------------------------------------------------------------

// Thread safe. Comprova si el nom de partida esta disponible.
// Retorna 0 si esta disponible, 1 si esta ocupat
int GameNameAvailable(GameTable* table, char name[GAME_LEN])
{
	pthread_mutex_lock(table->game_table_mutex);
	int available = __gameNameAvailable(table, name);
	pthread_mutex_unlock(table->game_table_mutex);
	return available;
}

// crea un usuari PreGameUser a partir d'un ConnectedUser per poder afegir-lo a la partida.
// El mutex li assignarem el de la taula de partides quan s'afegeixi la partida
PreGameUser* CreatePreGameUser(ConnectedUser* connectedUser)
{
	PreGameUser* preGameUser = malloc(sizeof(PreGameUser));
	pthread_mutex_lock(connectedUser->user_mutex);
	preGameUser->socket = connectedUser->socket;
	preGameUser->id = connectedUser->id;
	strcpy(preGameUser->charname,"none");
	preGameUser->userState = 0; //Defineix al usuari com a pendent de confirmaci�
	strcpy(preGameUser->username, connectedUser->username);
	pthread_mutex_unlock(connectedUser->user_mutex);
	return preGameUser;
}

// afegim un PreGameUser a la llista d'usuaris de la partida
int AddPreGameUser(PreGameState* gameState, PreGameUser* user)
{
	pthread_mutex_lock(gameState->game_mutex);
	if(gameState->userCount < MAX_GAME_USRCOUNT)
	{
		int pos = gameState->userCount;
		user->charId = pos;
		gameState->users[pos] = user;
		gameState->users[pos]->user_mutex = gameState->game_mutex;
		gameState->userCount++;		
		pthread_mutex_unlock(gameState->game_mutex);
		return 0;
	}
	else
	{
		pthread_mutex_unlock(gameState->game_mutex);
		return -1;
	}
}

// eliminem un usuari de la partida (p�blic)
int DeletePreGameUser(PreGameState* gameState, PreGameUser* user)
{
	pthread_mutex_lock(gameState->game_mutex);
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
		__deletePreGameUser(gameState->users[pos]); // alliberem el PreGameUser de la memoria
		for(int j = pos; j < userCount - 1; j++)
		{
			gameState->users[j] = gameState->users[j + 1];
		}
		gameState->userCount--;
		ret = 0;
	}
	else 
		ret = -1;
	pthread_mutex_unlock(gameState->game_mutex);
	return ret;
}

// aquesta funci� �s �til quan volem esborrar un usuari d'una partida, per� necessitem reassingar les id de character als usuaris restants de forma que siguin nombres sequencials.
int DeletePreGameUserWithCharIdResassignment(PreGameState* gameState, PreGameUser* user)
{
	pthread_mutex_lock(gameState->game_mutex);
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
		__deletePreGameUser(gameState->users[pos]); // alliberem el PreGameUser de la memoria
		for(int j = pos; j < userCount - 1; j++)
		{
			gameState->users[j] = gameState->users[j + 1];
		}
		gameState->userCount--;
		for (int j = 0; j < gameState->userCount; j++)
		{
			gameState->users[j]->charId = j;
		}
		ret = 0;
	}
	else 
		ret = -1;
	pthread_mutex_unlock(gameState->game_mutex);
	return ret;	
}

// creem una partida en estat PreGame (sala de partida on es poden unir altres jugadors)
// aquesta funci� inicialitza PreGameState amb el creador de la partida
// El mutex li assignarem el de la taula de partides quan afegim la partida a la taula
// Aquest mutex tamb� el compartirem amb els usuaris de la partida
PreGameState* CreateGame(ConnectedUser* user, char name[GAME_LEN])
{
	// no ens cal bloquejar el preGameState perque som els �nics utilitzant-lo
	PreGameState* preGameState = malloc(sizeof(PreGameState));
	preGameState->inGame = 0;								// partida en espera
	preGameState->gameId = -1; 								// la partida no esta a la taula de partides i no t� ID v�lida
	preGameState->userCount = 0;							// partida buida
	strcpy(preGameState->gameName, name); 					// el nom de la partida
	token_gen(preGameState->accessToken);					// el token de la partida
	
	// creem un PreGameUser a partir del ConnectedUser creador de la partida
	preGameState->creator = CreatePreGameUser(user);	
	preGameState->creator->userState = 1; //Defineix el creador com a jugador que ha "acceptat" jugar
	
	// creem el thread del joc
	pthread_mutex_t* mutexGame = malloc(sizeof(pthread_mutex_t));
	pthread_mutex_init(mutexGame, NULL);
	preGameState->game_mutex = mutexGame;
	
	// afegim el creador a la llista d'usuaris de la partida
	// encara no ens cal mutex perqu� encara no hem retornat preGameState
	// i a m�s els mutex no estan inicialitzats en la partida ni els jugadors
	__addPreGameUser(preGameState, preGameState->creator);	
	return preGameState;
}

// Afegim la partida a la llista de partides i assignem el mutex
// de la taula a la partida i als seus usuaris
// si la partida no es pot crear, l'eliminem.
int AddGameToGameTable(GameTable* table, PreGameState* gameState)
{
	int emptyPos = 0;
	int emptyFound = 0;
	int i = 0;
	int ret;
	pthread_mutex_lock(table->game_table_mutex);
	if (table->gameCount == MAX_GAMES)
	{
		__deletePreGame(gameState);
		ret = -1; // taula plena
	}
	else 
	{
		if(!__gameNameAvailable(table, gameState->gameName))
		{
			__deletePreGame(gameState);
			ret = -2; // Nom de partida ocupat
		}
		else
		{
			while (i < MAX_GAMES && !emptyFound)
			{
				// Busquem punter lliure
				if(table->createdGames[i] == NULL)
				{
					emptyFound = 1;
					emptyPos = i;
				}
				else
				   i++;
			}
			// Assignem id a la partida i la posem a la taula
			if (emptyFound)
			{
				gameState->gameId = emptyPos;
				
				// Assignem el mutex a la partida i als jugadors
				gameState->game_mutex = table->game_table_mutex; 
				for (int i = 0; i < gameState->userCount; i++)
				{
					gameState->users[i]->user_mutex = gameState->game_mutex;
				}
				gameState->creator->user_mutex = gameState->game_mutex;
				// Afegim la partida a la taula de partides
				table->createdGames[emptyPos] = gameState;
				table->gameCount++;
				ret = gameState->gameId;
			}
			else
			{
				__deletePreGame(gameState);
				ret = -1; // taula plena
			}	
		}		
	}
	pthread_mutex_unlock(table->game_table_mutex);
	return ret;
}

// Retorna 0 si ha esborrat la partida, -1 si no es troba a la taula.
int DeleteGameFromTable(GameTable* table, PreGameState* gameState)
{
	int ret;
	int gameIDpos;
	pthread_mutex_lock(table->game_table_mutex);
	gameIDpos = gameState->gameId;
	if(gameState->gameId != -1) // comprovem que la partida es troba a la llista amb ID valida
	{
		__deletePreGame(table->createdGames[gameState->gameId]); // hard delete, esborrem l'objecte per evitar leaks de memoria.
		table->createdGames[gameIDpos] = NULL; // posem el punter a NULL indicant espai lliure a la taula de partides
		ret = 0;
	}
	else
	   ret = -1;
	pthread_mutex_unlock(table->game_table_mutex);
	return ret;
}

// creem la taula de partides. Requeteix un mutex ja inicialitzat.
GameTable* CreateGameTable(pthread_mutex_t* mutex)
{
	GameTable* table = malloc(sizeof(GameTable));
	table->gameCount = 0;
	table->game_table_mutex = mutex;
	return table;
}

// m�tode per eliminar la taula de partides (p�blic)
void DeleteGameTable(GameTable* table)
{
	for (int i = 0; i < MAX_GAMES; i++)
	{
		if (table->createdGames[i] != NULL) 
		{
			__deletePreGame(table->createdGames[i]);
		}
		free(table);
	}
}

// assignaci� de personatge a l'usuari per a una partida (PreGame)
int PreGameAssignChar(PreGameState* gameState, char username[USRN_LENGTH], char charname[CHAR_LEN])
{
	pthread_mutex_lock(gameState->game_mutex);
	int userCount = gameState->userCount;
	int i = 0;
	int found = 0;
	while (i < userCount && !found)
	{
		if(!strcmp(gameState->users[i]->username, username))
		{
			found = 1;
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
	pthread_mutex_unlock(gameState->game_mutex);
	return ret;
}

// busca si tots han agafat personatge
int AllHasCharacter(PreGameState* preGameState)
{
	int found=1;
	int i=0;
	pthread_mutex_lock(preGameState->game_mutex);
	while((i<preGameState->userCount)&&(found))
	{
		if(!strcmp(preGameState->users[i]->charname,"none"))
			found=0;
		else
			i++;
	}
	pthread_mutex_unlock(preGameState->game_mutex);
	return found;
}

// busca si queden usuaris
int IamAloneinGame(PreGameState* preGameState)
{
	int i=0;
	int count = 0;
	pthread_mutex_lock(preGameState->game_mutex);
	for(i=0;i<preGameState->userCount;i++)
	{
		if((preGameState->users[i]->userState == 0)||(preGameState->users[i]->userState == 1))
		{
			++count;
		}
	}
	pthread_mutex_unlock(preGameState->game_mutex);
	int ret;
	if(count > 1)
		ret = 0;
	else
		ret = 1;
	return ret;
}

// busca l'usuari en la llista i dona la posici�
int GetPreGameUserPosByName(PreGameState* gameState, char username[USRN_LENGTH])
{
	//pthread_mutex_lock(gameState->game_mutex);
	int userCount = gameState->userCount;
	int i = 0;
	int found = 0;
	while ((i < userCount) && !found)
	{
		if(!strcmp(gameState->users[i]->username, username))
		{
			found = 1;
		}
		else 
		    i++;
	}
	//pthread_mutex_unlock(gameState->game_mutex);
	if (!found)
		i = -1;
	return i;
}

// busca la partida en la taula de partides a partir del nom
PreGameState* GetPreGameStateByName (GameTable* gameTable, char gameName [GAME_LEN])
{
	pthread_mutex_lock(gameTable->game_table_mutex);
	PreGameState* ret;
	int i = 0;
	int found = 0;
	while((i < gameTable->gameCount)&&(!found))
	{
		if(gameTable->createdGames[i] != NULL)
		{
			if(!strcmp(gameName,gameTable->createdGames[i]->gameName))
				found = 1;
			else
				i++;
		}
	}
	
	if(found)
	{
		ret = gameTable->createdGames[i];
	}
	else
    {
		ret = NULL;
	}
	pthread_mutex_unlock(gameTable->game_table_mutex);
	return ret;
}

// Passem tota la taula de partides a qui la demani en format json.
json_object* GameTableToJson(GameTable* table)
{
	json_object* gameJson = json_object_new_array();
	pthread_mutex_lock(table->game_table_mutex);	
	for(int i = 0; i < MAX_GAMES; i++)
	{
		if (table->createdGames[i] != NULL)
		{
			json_object* game = json_object_new_object();
			
			json_object* id = json_object_new_int(table->createdGames[i]->gameId);
			json_object* name = json_object_new_string(table->createdGames[i]->gameName);		
			json_object* creator = json_object_new_string(table->createdGames[i]->creator->username);
			json_object* userCount = json_object_new_int(table->createdGames[i]->userCount);		
			json_object* playing = json_object_new_int(table->createdGames[i]->inGame);
			
			json_object_object_add(game, "Id", id);
			json_object_object_add(game, "Name", name);
			json_object_object_add(game, "Creator", creator);
			json_object_object_add(game, "UserCount", userCount);
			json_object_object_add(game, "Playing", playing);
			
			json_object_array_add(gameJson, game);	
		}
	}	
	pthread_mutex_unlock(table->game_table_mutex);	
	return gameJson;
}

//Generaem un json que inclou l'estat d'una partida.
json_object* GameStateToJson(PreGameState* preGameState)
{
	json_object* gameJson = json_object_new_array();
	pthread_mutex_lock(preGameState->game_mutex);	
	for(int i = 0; i < preGameState->userCount; i++)
	{
		if (preGameState->users[i] != NULL)
		{
			json_object* user = json_object_new_object();
			
			json_object* id = json_object_new_int(preGameState->users[i]->id);
			json_object* username = json_object_new_string(preGameState->users[i]->username);		
			json_object* charname = json_object_new_string(preGameState->users[i]->charname);
			json_object* userState = json_object_new_int(preGameState->users[i]->userState);		
			
			json_object_object_add(user, "Id", id);
			json_object_object_add(user, "UserName", username);
			json_object_object_add(user, "CharName", charname);
			json_object_object_add(user, "UserState", userState);
			
			json_object_array_add(gameJson, user);	
		}
	}	
	pthread_mutex_unlock(preGameState->game_mutex);	
	return gameJson;
}

// generem un JSON que cont� par�metres d'incialitzaci� de la partida. Aquesta �s la informaci� que respon el servidor a una petici� de tipus 101/HELLO.
// S'envia sempre de forma individual
json_object* GameInitStateJson(PreGameState* preGameState, int userId)
{
	int formatOk = 0;
	json_object* initState = json_object_new_object();
	//json_object* thisUser = json_object_new_object();
	json_object* users = json_object_new_array();
	
	json_object* gameName = json_object_new_string(preGameState->gameName);
	json_object* nPlayers = json_object_new_int(preGameState->userCount);
	
	json_object_object_add(initState, "gameName", gameName);
	json_object_object_add(initState, "nPlayers", nPlayers);
	
	pthread_mutex_lock(preGameState->game_mutex);	
	for(int i = 0; i < preGameState->userCount; i++)
	{
		if (preGameState->users[i] != NULL)
		{
			if (preGameState->users[i]->userState == 1)
			{
				json_object* user = json_object_new_object();
				
				json_object* userid = json_object_new_int(preGameState->users[i]->id);
				json_object* charid = json_object_new_int(preGameState->users[i]->charId);
				json_object* username = json_object_new_string(preGameState->users[i]->username);		
				json_object* charname = json_object_new_string(preGameState->users[i]->charname);
				
				json_object_object_add(user, "userId", userid);
				json_object_object_add(user, "charId", charid);
				json_object_object_add(user, "userName", username);
				json_object_object_add(user, "charName", charname);
				
				json_object_array_add(users, user);	
				
				if (preGameState->users[i]->id == userId)
				{
					formatOk = 1;
					json_object_object_add(initState, "thisUser", user);
				}
			}			
		}
	}	
	pthread_mutex_unlock(preGameState->game_mutex);	
	json_object_object_add(initState, "users", users);
	if (formatOk)
	{
		return initState;
	}
	else return NULL;
}

// obttenim el char id d'un personatge a partir de la seva id d'usuari
int GetCharIdFromUserId(PreGameState* state, int userId)
{
	int found = 0;
	int i = 0;
	int ret;
	pthread_mutex_lock(state->game_mutex);
	while (!found && i < state->userCount)
	{
		if (userId == state->users[i]->id)
		{
			found = 1;
		}
		else ++i;
	}
	if (found)
	{
		ret = state->users[i]->charId;
	}
	else ret = -1;
	pthread_mutex_unlock(state->game_mutex);
	return ret;
}

// obttenim el nom d'usuari d'un personatge a partir de la seva id de character en una partida.
void GetUsernameFromCharId(PreGameState* state, int charId, char username[USRN_LENGTH])
{
	int found = 0;
	int i = 0;
	pthread_mutex_lock(state->game_mutex);
	while (!found && i < state->userCount)
	{
		if (charId == state->users[i]->charId)
		{
			found = 1;
		}
		else ++i;
	}
	if (found)
	{
		strcpy(username, state->users[i]->username);
	}
	else username = NULL;
	pthread_mutex_unlock(state->game_mutex);
}

// obtenim l'id de l'usuari a partir de la seva char id en una partida concreta.
int GetUserIdFromCharId(PreGameState* state, int charId)
{
	int ret;
	int found = 0;
	int i = 0;
	pthread_mutex_lock(state->game_mutex);
	while (!found && i < state->userCount)
	{
		if (charId == state->users[i]->charId)
		{
			found = 1;
		}
		else ++i;
	}
	if (found)
	{
		ret = state->users[i]->id;
	}
	else ret = -1;
	pthread_mutex_unlock(state->game_mutex);
	return ret;
}
//------------------------------------------------------------------------------

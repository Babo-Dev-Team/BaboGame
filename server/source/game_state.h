#ifndef GAME_STATE_H
#define GAME_STATE_H
#include "globals.h"
#include "json.h"

typedef struct CharacterState{
	int characterId;
	int position_X;
	int position_Y;
	int velocity_X;
	int velocity_Y;
}CharacterState;

typedef struct GameState{
	int gameID;
	int playable;
	int n_players;
	CharacterState* characterStatesList;
	json_object* gameStateJson;
	
}GameState;

GameState* CreateGameState(int gameId, int n_players);
void DeleteGameState(GameState* game);
void UpdateGameStateJson(GameState* game);

void SetInitialPositions (GameState* game, int** positions);







#endif

#ifndef GAME_STATE_H
#define GAME_STATE_H
#include "globals.h"
#include "json.h"

typedef struct ProjectileState{
	int projectileID;
	int shooterID;
	char projectileType;
	int position_X;
	int position_Y;
	double direction_X;
	double direction_Y;
	double LinearVelocity;
	int hitCount;
	int target_X;
	int target_Y;
}ProjectileState;


typedef struct CharacterState{
	int characterId;
	int position_X;
	int position_Y;
	int velocity_X;
	int velocity_Y;
	double direction_X;
	double direction_Y;
	int health;
	ProjectileState projectileStates[PROJ_COUNT_PLAYER];
	int projectileCount;
}CharacterState;

typedef struct GameState{
	int gameID;
	int playable;
	int n_players;
	//int projectileCount;
	CharacterState* characterStatesList;
	//ProjectileState* projectileStates;
	json_object* gameStateJson;
	
}GameState;

typedef struct playerUpdate{
	CharacterState* characterState;
	ProjectileState* projectileStates;
}playerUpdate;

GameState* CreateGameState(int gameId, int n_players);
void DeleteGameState(GameState* game);
void UpdateGameStateJson(GameState* game);

void SetInitialPositions (GameState* game, int** positions);







#endif

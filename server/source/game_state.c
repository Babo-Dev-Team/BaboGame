#include "game_state.h"
#include "json.h"

// funció per crear i inicialitzar l'estat global d'una partida. Aquesta funció es fa servir 
// per generar els estats de partida dels Game Processors, i com que només els threads Game Processor
// hi tindran accés, no cal implementan mecanismes d'exclusió mútua en aquesta estructura d'estat.
GameState* CreateGameState(int gameId, int n_players)
{
	GameState* game = malloc(sizeof(GameState));
	game->gameID = gameId;
	game->playable = 0;
	game->characterStatesList = malloc(n_players * sizeof(CharacterState));
	for (int i = 0; i < n_players; i++)
	{
		game->characterStatesList[i].characterId = i;
		game->characterStatesList[i].position_X = 0;
		game->characterStatesList[i].position_Y = 0;
		game->characterStatesList[i].velocity_X = 0;
		game->characterStatesList[i].velocity_Y = 0;
		game->characterStatesList[i].direction_X = 0;
		game->characterStatesList[i].direction_Y = 0;
		game->characterStatesList[i].health = 20;
		game->characterStatesList[i].projectileCount = 0;
		for (int j = 0; j < PROJ_COUNT_PLAYER; j++)
		{
			game->characterStatesList[i].projectileStates[j].direction_X = 0;
			game->characterStatesList[i].projectileStates[j].direction_Y = 0;
			game->characterStatesList[i].projectileStates[j].position_X = 0;
			game->characterStatesList[i].projectileStates[j].position_Y = 0;
			game->characterStatesList[i].projectileStates[j].LinearVelocity = 0;
			game->characterStatesList[i].projectileStates[j].hitCount = 0;
			game->characterStatesList[i].projectileStates[j].projectileID = 0;
			game->characterStatesList[i].projectileStates[j].shooterID = 0;
			game->characterStatesList[i].projectileStates[j].projectileType = 'N';
			game->characterStatesList[i].projectileStates[j].target_X = 0;
			game->characterStatesList[i].projectileStates[j].target_Y = 0;
		}
	}
	game->n_players = n_players;

	game->gameStateJson = json_object_new_object();
	json_object* chars_array = json_object_new_array();
	json_object* projStates_array = json_object_new_array();
	
	json_object* gameID = json_object_new_int(game->gameID);
	json_object_object_add(game->gameStateJson, "gameID", gameID);
	
	json_object* playable = json_object_new_int(game->playable);
	json_object_object_add(game->gameStateJson, "playable", playable);
	
	json_object* nPlayers = json_object_new_int(game->n_players);
	json_object_object_add(game->gameStateJson, "nPlayers", nPlayers);
	
	json_object* characters[n_players];
	
	for(int i = 0; i < n_players; i++)
	{
		characters[i] = json_object_new_object();
		json_object* charId = json_object_new_int(game->characterStatesList[i].characterId);
		json_object* posX = json_object_new_int(game->characterStatesList[i].position_X);
		json_object* posY = json_object_new_int(game->characterStatesList[i].position_Y);
		json_object* velX = json_object_new_int(game->characterStatesList[i].velocity_X);
		json_object* velY = json_object_new_int(game->characterStatesList[i].velocity_Y);
		json_object* dirX = json_object_new_double(game->characterStatesList[i].direction_X);
		json_object* dirY = json_object_new_double(game->characterStatesList[i].direction_Y);
		json_object* health = json_object_new_int(game->characterStatesList[i].health);
		json_object_object_add(characters[i], "charID", charId);
		json_object_object_add(characters[i], "posX", posX);
		json_object_object_add(characters[i], "posY", posY);
		json_object_object_add(characters[i], "velX", velX);
		json_object_object_add(characters[i], "velY", velY);
		json_object_object_add(characters[i], "dirX", dirX);
		json_object_object_add(characters[i], "dirY", dirY);
		json_object_object_add(characters[i], "health", health);
	}
	
	for (int i = 0; i < n_players; i++)
	{
		json_object_array_add(chars_array, characters[i]);
	}
	
	json_object_object_add(game->gameStateJson, "characterStatesList", chars_array);	
	json_object_object_add(game->gameStateJson, "projectileStates", projStates_array);
	return game;
}

// eliminem l'estat de partida i alliberem la memòria assignada de forma dinàmica.
void DeleteGameState(GameState* game)
{
	free(game->characterStatesList);
	free(game);
}

// actualitzem el JSON que representa l'estat de la partida per a que incorpori l'estat més recent. Aquest 
// objecte json és el que s'envia als clients
void UpdateGameStateJson(GameState* game)
{
	int err;
	
	json_object* gameID;
	err = json_object_object_get_ex(game->gameStateJson, "gameId", &gameID);
	
	json_object* playable;
	err = json_object_object_get_ex(game->gameStateJson, "playable", &playable);
	
	json_object* nPlayers;
	err = json_object_object_get_ex(game->gameStateJson, "nPlayers", &nPlayers);	
	
	json_object_set_int(playable, game->playable);
	json_object_set_int(nPlayers, game->n_players);
	json_object_set_int(gameID, game->gameID);
	
	json_object* charArray;
	err = json_object_object_get_ex(game->gameStateJson, "characterStatesList", &charArray);

	json_object* charState;
	
	json_object* charId;
	json_object* posX;
	json_object* posY;
	json_object* velX;
	json_object* velY;
	json_object* dirX;
	json_object* dirY;
	json_object* health;
	for (int i = 0; i < game->n_players; i++)
	{
		charState = json_object_array_get_idx(charArray, i);
		
		json_object_object_get_ex(charState, "charID", &charId);	
		json_object_object_get_ex(charState, "posX", &posX);	
		json_object_object_get_ex(charState, "posY", &posY);	
		json_object_object_get_ex(charState, "velX", &velX);	
		json_object_object_get_ex(charState, "velY", &velY);
		json_object_object_get_ex(charState, "dirX", &dirX);
		json_object_object_get_ex(charState, "dirY", &dirY);
		json_object_object_get_ex(charState, "health", &health);
		
		json_object_set_int(charId, game->characterStatesList[i].characterId);
		json_object_set_int(posX, game->characterStatesList[i].position_X);
		json_object_set_int(posY, game->characterStatesList[i].position_Y);
		json_object_set_int(velX, game->characterStatesList[i].velocity_X);
		json_object_set_int(velY, game->characterStatesList[i].velocity_Y);
		json_object_set_double(dirX, game->characterStatesList[i].direction_X);
		json_object_set_double(dirY, game->characterStatesList[i].direction_Y);
		json_object_set_int(health, game->characterStatesList[i].health);
	}
	
	//json_object* oldProjStatesArray;
	//err = json_object_object_get_ex(game->gameStateJson, "projectileStates", &oldProjStatesArray);
	json_object_object_del(game->gameStateJson, "projectileStates");	
	json_object* projStates_array = json_object_new_array();
	
	for (int i = 0; i < game->n_players; i++)
	{
		json_object* projectiles[PROJ_COUNT_PLAYER];
		for (int j = 0; j < game->characterStatesList[i].projectileCount; j++)
		{
			projectiles[j] = json_object_new_object();
			
			json_object* projectileId = json_object_new_int(game->characterStatesList[i].projectileStates[j].projectileID);
			json_object* shooterId = json_object_new_int(game->characterStatesList[i].projectileStates[j].shooterID);
			json_object* projectileType = json_object_new_string_len(&game->characterStatesList[i].projectileStates[j].projectileType, 1);
			json_object* projectilePosX = json_object_new_int(game->characterStatesList[i].projectileStates[j].position_X);
			json_object* projectilePosY = json_object_new_int(game->characterStatesList[i].projectileStates[j].position_Y);
			json_object* projectileDirX = json_object_new_double(game->characterStatesList[i].projectileStates[j].direction_X);
			json_object* projectileDirY = json_object_new_double(game->characterStatesList[i].projectileStates[j].direction_Y);
			json_object* projectileLinearVelocity = json_object_new_double(game->characterStatesList[i].projectileStates[j].LinearVelocity);
			json_object* hitCount = json_object_new_int(game->characterStatesList[i].projectileStates[j].hitCount);
			json_object* projectileTarX = json_object_new_int(game->characterStatesList[i].projectileStates[j].target_X);
			json_object* projectileTarY = json_object_new_int(game->characterStatesList[i].projectileStates[j].target_Y);
			
			json_object_object_add(projectiles[j], "projectileID", projectileId);
			json_object_object_add(projectiles[j], "shooterID", shooterId);
			json_object_object_add(projectiles[j], "projectileType", projectileType);
			json_object_object_add(projectiles[j], "posX", projectilePosX);
			json_object_object_add(projectiles[j], "posY", projectilePosY);
			json_object_object_add(projectiles[j], "directionX", projectileDirX);
			json_object_object_add(projectiles[j], "directionY", projectileDirY);
			json_object_object_add(projectiles[j], "LinearVelocity", projectileLinearVelocity);
			json_object_object_add(projectiles[j], "hitCount", hitCount);
			json_object_object_add(projectiles[j], "targetX", projectileTarX);
			json_object_object_add(projectiles[j], "targetY", projectileTarY);
		}
		for (int j = 0; j < game->characterStatesList[i].projectileCount; j++)
		{
			json_object_array_add(projStates_array, projectiles[j]);
		}
	}
	json_object_object_add(game->gameStateJson, "projectileStates", projStates_array);
}

void SetInitialPositions (GameState* game, int** positions)
{
	for (int i = 0; i < game->n_players; i++)
	{
		game->characterStatesList[i].position_X = positions[i][0];
		game->characterStatesList[i].position_Y = positions[i][1];
	}
}




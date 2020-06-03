#include "game_state.h"
#include "json.h"


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
	}
	game->n_players = n_players;
	
	game->gameStateJson = json_object_new_object();
	json_object* chars_array = json_object_new_array();
	
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
	return game;
}

void DeleteGameState(GameState* game)
{
	free(game->characterStatesList);
	free(game);
}

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
}

void SetInitialPositions (GameState* game, int** positions)
{
	for (int i = 0; i < game->n_players; i++)
	{
		game->characterStatesList[i].position_X = positions[i][0];
		game->characterStatesList[i].position_Y = positions[i][1];
	}
}




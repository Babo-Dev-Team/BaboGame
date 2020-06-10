#ifndef BBDD_HANDLER_H
#define BBDD_HANDLER_H

#include "globals.h"

int BBDD_connect();
char* BBDD_ranking();
char* BBDD_time_played(char username[USRN_LENGTH]);
char* BBDD_find_characters(char game_id[GAME_ID_LENGTH]);
int BBDD_add_user(char username[USRN_LENGTH], char passwd[PASS_LENGTH]);
int BBDD_check_login (char username[USRN_LENGTH], char passwd[PASS_LENGTH]);
char* BBDD_opponentGameList(int idPlayer);
char* BBDD_gameResultsWithOtherPlayers (int num,char players [100][USRN_LENGTH]);
char* BBDD_gameInTimeInterval(int idPlayer,char start[100], char end [100]);
int send_query();

int BBDD_deregister_user(char username[USRN_LENGTH], char password[PASS_LENGTH]);
int BBDD_add_game_scores(char name[GAME_LEN], int nPlayers, char** charnames, int* userIds, int* scores, int winnerId, char* initDate, char* endDate, int duration);

#endif

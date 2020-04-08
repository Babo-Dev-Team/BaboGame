#ifndef BBDD_HANDLER_H
#define BBDD_HANDLER_H

#include "globals.h"

int BBDD_connect();
char* BBDD_ranking();
char* BBDD_time_played(char username[USRN_LENGTH]);
char* BBDD_find_characters(char game_id[GAME_ID_LENGTH]);
int BBDD_add_user(char username[USRN_LENGTH], char passwd[PASS_LENGTH]);
int BBDD_check_login (char username[USRN_LENGTH], char passwd[PASS_LENGTH]);
int send_query();

#endif

#ifndef BBDD_HANDLER_H
#define BBDD_HANDLER_H

#define USRN_LENGTH 32
#define GAME_LENGTH 32
#define PASS_LENGTH 32

//#include <mysql.h>

int BBDD_connect();
char* BBDD_find_winner(char game[GAME_LENGTH]);
char* BBDD_time_played(char username[USRN_LENGTH]);
char* BBDD_find_characters(char game[GAME_LENGTH]);
int BBDD_add_user(char username[USRN_LENGTH], char passwd[PASS_LENGTH]);
int BBDD_check_login (char username[USRN_LENGTH], char passwd[PASS_LENGTH]);
int send_query();



#endif

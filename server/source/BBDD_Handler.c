#include <stdlib.h>
#include <string.h>
#include <mysql.h>
#include <stdio.h>

#include "BBDD_Handler.h"

#define QUERY_LENGTH 500

// Mysql data structures
MYSQL *conn;
MYSQL_RES *result;
MYSQL_ROW row;


// PROVISIONAL:
int user_id = 10;


int send_query(char query[QUERY_LENGTH])
{
	// send query
	int error = mysql_query(conn, query);
	if (error != 0)
	{
		printf ("Query Error:  %u %s\n", mysql_errno(conn), mysql_error(conn));
		return -1;
	}
	result = mysql_store_result(conn);
	return 0;
}

char* BBDD_find_winner(char game[GAME_LENGTH])
{
	
}

char* BBDD_time_played(char username[USRN_LENGTH])
{
	
}

char* BBDD_find_characters(char game[GAME_LENGTH])
{
	
}

int BBDD_add_user(char username[USRN_LENGTH], char passwd[PASS_LENGTH])
{
	char query[QUERY_LENGTH];
	
	strcpy(query, "select MAX(jugadors.id) from jugadors");
	int error = send_query(query);
	row = mysql_fetch_row(result);
	
	if (row == NULL)
	{
		//no users yet
		user_id = 1;
	}
	else
	{
		user_id = atoi(row[0]) + 1;
	}
	
	sprintf(query, "insert into jugadors values ( %d", user_id);
	strcat(query, ", '");
	strcat(query, username);
	strcat(query, "', '");
	strcat(query, passwd);
	strcat(query, "')");
	
	error = mysql_query(conn, query);
	
	//TO DO: afegir comprovacio d-usuari prexistent
	
	return 0;
	
}

int BBDD_check_login (char username[USRN_LENGTH], char passwd[PASS_LENGTH])
{
	char query[QUERY_LENGTH];
	
	strcpy(query, "select jugadors.id from jugadors where jugadors.nom = '");
	strcat(query, username);
	strcat(query, "' and jugadors.passwd = '");
	strcat(query, passwd);
	strcat(query, "'");

	
	int error = send_query(query);
	
	row = mysql_fetch_row(result);
	
	if (row == NULL)
	{
		// login FAIL
		return -1;
	}
	else
	{
		// login OK
		return 0;
	}
}

int BBDD_connect ()
{
	int error = 0;
	// create MQYSL server conn
	conn = mysql_init(NULL);
	if(conn == NULL)
	{
		printf("Error while creating the connection: %u, %s\n", mysql_errno(conn), mysql_error(conn));
		error = -1;
	}
	
	// init connection
	conn = mysql_real_connect (conn, "localhost","root", "mysql", "BaboGameBBDD", 0, NULL, 0);
	if (conn == NULL) 
	{
		printf ("Error while initializing connection: %u %s\n", mysql_errno(conn), mysql_error(conn));
		error = -2;
	}
	
	return error;
}


/*int main(int argc, char **argv)
{
	// Mysql data structures
	MYSQL *conn;
	MYSQL_RES *result;
	MYSQL_ROW row;
	
	int err;
	char query[500];
	
	// create MQYSL server conn
	conn = mysql_init(NULL);
	if(conn == NULL)
	{
		printf("Error while creating the connection: %u, %s\n", mysql_errno(conn), mysql_error(conn));
		exit(1); // exit process with code 1
	}
	
	// init connection
	conn = mysql_real_connect (conn, "localhost","root", "mysql", "BaboGameBBDD", 0, NULL, 0);
	if (conn == NULL) 
	{
		printf ("Error while initializing connection: %u %s\n", mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
	
	char name[32];
	
	printf("Type the player's name:");
	scanf("%s", name);
	
	// SQL Query
	//strcpy(query, "SELECT SUM(TIMESTAMPDIFF(SECOND, partides.dataInici, partides.dataFinal)) FROM jugadors,participants,partides WHERE ");
	strcpy(query, "SELECT SUM(partides.duracio) FROM jugadors,participants,partides WHERE ");
	strcat(query, "jugadors.nom = '");
	strcat(query, name);
	strcat(query, "' AND jugadors.id = participants.idJugador ");
	strcat(query, "AND participants.idPartida = partides.id");
	
	// send query
	err = mysql_query(conn, query);
	if (err != 0)
	{
		printf ("Query Error:  %u %s\n", mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
	
	// store query result
	result = mysql_store_result(conn);
	row = mysql_fetch_row(result);
	
	// the SUM function returns NULL if no values are added.
	// In this case, row[0] will be null
	if (row == NULL || row[0] == NULL)
	{
		printf("Query result contains no data\n");
	}
	else
	{
		// Convert time in seconds to HH:MM:SS format
		int raw_time = atoi(row[0]);
		int seconds = raw_time % 60;
		int minutes = ((raw_time - seconds) / 60) % 60;
		int hours = ((raw_time - seconds - (minutes * 60)) / 3600) % 24;
		printf("Total time played by %s: %d:%d:%d\n", name, hours, minutes, seconds);
	}
}*/


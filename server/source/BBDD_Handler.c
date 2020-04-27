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

char* BBDD_ranking()
{
	char query[QUERY_LENGTH];	
	char* ranking_str = malloc(sizeof(char) * 10000);
	char ranking_rows[100][100];
	
	strcpy(query,"SELECT jugadors.nom, COUNT(*) FROM partides, jugadors WHERE jugadors.id=partides.idGuanyador GROUP BY partides.idGuanyador ORDER BY COUNT(*) DESC LIMIT 10;");
	int error = send_query(query);		
	
	row = mysql_fetch_row(result);	
	if (row == NULL)
	{
		//printf("No s'han trobat dades per a la consulta\n");
		strcpy(ranking_str, "/0/");
	}
	else
	{
		char player[USRN_LENGTH];
		char games_won[4];
		int i = 0;
		while (row != NULL)
		{
			strcpy(player, row[0]);
			strcpy(games_won, row[1]);
			sprintf(ranking_rows[i], "%s*%s/", player, games_won);
			i++;
			row = mysql_fetch_row(result);
		}
		sprintf(ranking_str, "%d/", i);
		for(int j = 0; j < i; j++)
		{
			strcat(ranking_str, ranking_rows[j]);
		}
	}
	return ranking_str;	
}

char* BBDD_time_played(char username[USRN_LENGTH])
{
	// SQL Query
	char query[QUERY_LENGTH];
	strcpy(query, "SELECT SUM(partides.duracio) FROM jugadors,participants,partides WHERE ");
	strcat(query, "jugadors.nom = '");
	strcat(query, username);
	strcat(query, "' AND jugadors.id = participants.idJugador ");
	strcat(query, "AND participants.idPartida = partides.id");
	
	int error = send_query(query);
	row = mysql_fetch_row(result);
	
	char* time_played = malloc(sizeof(char) * 20);
	
	// the SUM function returns NULL if no values are added.
	// In this case, row[0] will be null
	if (row == NULL || row[0] == NULL)
	{
		strcpy(time_played, "00:00:00");
	}
	else
	{
		// Convert time in seconds to HH:MM:SS format
		int raw_time = atoi(row[0]);
		int seconds = raw_time % 60;
		int minutes = ((raw_time - seconds) / 60) % 60;
		int hours = ((raw_time - seconds - (minutes * 60)) / 3600) % 24;
		sprintf(time_played, "%d:%d:%d\n", hours, minutes, seconds);
	}
	return time_played;
}

char* BBDD_find_characters(char game_id[GAME_ID_LENGTH])
{
	char query[QUERY_LENGTH];	
	char* characters_str = malloc(sizeof(char) * 10000);
	char character_rows[100][100];
	
	strcpy(query, "SELECT jugadors.nom, participants.personatge FROM jugadors,participants WHERE (jugadors.id = participants.idJugador) AND (participants.idPartida = '");
	strcat(query, game_id);
	strcat(query,"')");
		
	int error = send_query(query);		
	
	row = mysql_fetch_row(result);	
	if (row == NULL)
	{
		//printf("No s'han trobat dades per a la consulta\n");
		strcpy(characters_str, "/0/");
	}
	else
	{
		char player[USRN_LENGTH];
		char character[USRN_LENGTH];
		int i = 0;
		while (row != NULL)
		{
			strcpy(player, row[0]);
			strcpy(character, row[1]);
			sprintf(character_rows[i], "%s*%s/", player, character);
			i++;
			row = mysql_fetch_row(result);
		}
		sprintf(characters_str, "%d/", i);
		for(int j = 0; j < i; j++)
		{
			strcat(characters_str, character_rows[j]);
		}
	}
	return characters_str;		
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
	
	return user_id;
	
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
		return atoi(row[0]);
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
	conn = mysql_real_connect (conn, "shiva2.upc.es","root", "mysql", "T12_BaboGameBBDD", 0, NULL, 0);
	if (conn == NULL) 
	{
		printf ("Error while initializing connection: %u %s\n", mysql_errno(conn), mysql_error(conn));
		error = -2;
	}
	
	return error;
}

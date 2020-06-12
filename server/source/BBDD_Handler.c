#include <mysql.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>

#include "BBDD_Handler.h"

#define QUERY_LENGTH 4096

// Mysql data structures
MYSQL *conn;
MYSQL_RES *result;
MYSQL_ROW row;




// funció per enviar una query al servidor SQL
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

// demanem el ranking de jugadors, que retornem com un punter a una string
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

// demanem el temps jugat per un jugador. Retornem un punter a una string amb el temps en format 
// HH:MM:SS
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
		sprintf(time_played, "%d:%d:%d", hours, minutes, seconds);
	}
	return time_played;
}

// busquem els personatges que han participat en una partida pel nom de partida.
// retornem una string de la forma num/jugador1*char1/jugador2*char2...
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

// mètode de sign up al joc
int BBDD_add_user(char username[USRN_LENGTH], char passwd[PASS_LENGTH])
{
	int user_id;
	char query[QUERY_LENGTH];
	
	strcpy(query, "select jugadors.id from jugadors where jugadors.nom = '");
	strcat(query, username);
	strcat(query, "'");
	
	int error = send_query(query);
	row = mysql_fetch_row(result);
	if (row != NULL)
	{
		// this user already exists!!!
		return -1;
	}
	else
	{
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
		strcat(query, "',1)");
		
		error = mysql_query(conn, query);
		return user_id;		
	}
}

// comprovem els paràmetre de login d'un usuari. retorna la id de l'usuari si el longin 
// és correcte, o -1 si el login no és correcte.
int BBDD_check_login (char username[USRN_LENGTH], char passwd[PASS_LENGTH])
{
	char query[QUERY_LENGTH];
	
	strcpy(query, "select jugadors.id from jugadors where jugadors.nom = '");
	strcat(query, username);
	strcat(query, "' and jugadors.passwd = '");
	strcat(query, passwd);
	strcat(query, "' and jugadors.actiu = 1");
	
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

// ens connectem al servidor sql
// retorna 0 si la connexió és correcta, -1 si no s'ha pogut crear la connexió, 
// -2 si no s'ha pogut inicialitzar la connexió.
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
	//conn = mysql_real_connect (conn, "localhost","root", "mysql", "T12_BaboGameBBDD", 0, NULL, 0);
	if (conn == NULL) 
	{
		printf ("Error while initializing connection: %u %s\n", mysql_errno(conn), mysql_error(conn));
		error = -2;
	}
	
	return error;
}

// retorna una llistade jugadors contra els que ha jugat un usuari especificat per la seva id.
char* BBDD_opponentGameList(int idPlayer)
{
	char query[QUERY_LENGTH];	
	char* opponent_str = malloc(sizeof(char) * 10000);
	char opponent_rows[100][100];
	
	strcpy(query, "SELECT DISTINCT jugadors.nom, jugadors.id, jugadors.actiu FROM jugadors,participants WHERE (jugadors.id = participants.idJugador) AND (jugadors.id != ");
	sprintf(query,"%s%d",query,idPlayer);
	strcat(query, ") AND (participants.idPartida IN (SELECT participants.idPartida FROM participants WHERE (participants.idJugador = ");
	sprintf(query,"%s%d",query,idPlayer);
	strcat(query,")))");
	
	int error = send_query(query);		
	
	row = mysql_fetch_row(result);	
	if (row == NULL)
	{
		//printf("No s'han trobat dades per a la consulta\n");
		strcpy(opponent_str, "0/");
	}
	else
	{
		char player[USRN_LENGTH];
		int id;
		int actiu;
		int i = 0;
		while (row != NULL)
		{
			strcpy(player, row[0]);
			id = atoi(row[1]);
			actiu = atoi(row[2]);
			sprintf(opponent_rows[i], "%s*%d*%d/", player, id, actiu);
			i++;
			row = mysql_fetch_row(result);
		}
		sprintf(opponent_str, "%d/", i);
		for(int j = 0; j < i; j++)
		{
			strcat(opponent_str, opponent_rows[j]);
		}
	}
	return opponent_str;
}


char* BBDD_gameResultsWithOtherPlayers (int num,char players [100][USRN_LENGTH])
{
	char query[QUERY_LENGTH];	
	char* gameResults_str = malloc(sizeof(char) * 10000);
	char gameResults_rows[100][100];
	int idWinners [100];
	char gameWinner_rows[100][100];
		
	strcpy(query,"SELECT partides.nom, partides.id, partides.idGuanyador FROM partides ");
	if (num > 0)
	{
		strcat(query, "WHERE");
		for(int i = 0; i < num; i++)
		{
			if(i != 0)
				strcat(query, " AND");
			
			strcat(query, " (partides.id IN (SELECT participants.idPartida FROM participants, jugadors WHERE (jugadors.id = participants.idJugador)AND(jugadors.nom = '");
			strcat(query, players[i]);
			strcat(query, "')))");
		}
	}
	
	
	int error = send_query(query);		
	
	row = mysql_fetch_row(result);	
	if (row == NULL)
	{
		//printf("No s'han trobat dades per a la consulta\n");
		strcpy(gameResults_str, "0/");
	}
	else
	{
		char player[USRN_LENGTH];
		int id;
		int i = 0;
		while (row != NULL)
		{
			strcpy(player, row[0]);
			id = atoi(row[1]);
			idWinners[i] = atoi(row[2]);
			sprintf(gameResults_rows[i], "%s*%d", player, id);
			i++;
			row = mysql_fetch_row(result);
		}
		for(int k = 0; k < i; k++)
		{
			strcpy(query, "SELECT nom FROM jugadors WHERE (id = ");
			sprintf(query,"%s%d",query,idWinners[k]);
			strcat(query,")");
			int error = send_query(query);		
			
			row = mysql_fetch_row(result);	
			if (row == NULL)
			{
				//printf("No s'han trobat dades per a la consulta\n");
				strcpy(gameWinner_rows[k], "none");
			}
			else
			{
				while (row != NULL)
				{
					strcpy(gameWinner_rows[k], row[0]);
					row = mysql_fetch_row(result);
				}
			}
		}
		sprintf(gameResults_str, "%d/", i);
		for(int j = 0; j < i; j++)
		{
			sprintf(gameResults_rows[j],"%s*%s/",gameResults_rows[j],gameWinner_rows[j]);
			strcat(gameResults_str, gameResults_rows[j]);
		}
	}
	return gameResults_str;
}

// retorna com a punter a una string una llista de partides jugades per un jugador dins d'un marge de temps.
char* BBDD_gameInTimeInterval(int idPlayer,char start[100], char end [100])
{
	char query[QUERY_LENGTH];	
	char* interval_str = malloc(sizeof(char) * 10000);
	char interval_rows[100][100];
	
	strcpy(query, "SELECT partides.nom, partides.id, partides.dataInici FROM partides,participants WHERE (participants.idJugador = ");
	sprintf(query,"%s%d",query,idPlayer);
	strcat(query, ") AND (participants.idPartida = partides.id) AND (partides.dataInici < ' ");
	strcat(query,end);
	strcat(query, " ') AND (partides.dataInici > ' ");
	strcat(query,start);
	strcat(query,"')");
	
	int error = send_query(query);		
	
	row = mysql_fetch_row(result);	
	if (row == NULL)
	{
		//printf("No s'han trobat dades per a la consulta\n");
		strcpy(interval_str, "0/");
	}
	else
	{
		char game[GAME_LEN];
		int id;
		char dateTime[100];
		int i = 0;
		while (row != NULL)
		{
			strcpy(game, row[0]);
			id = atoi(row[1]);
			strcpy(dateTime, row[2]);
			sprintf(interval_rows[i], "%s*%d*%s/", game, id, dateTime);
			i++;
			row = mysql_fetch_row(result);
		}
		sprintf(interval_str, "%d/", i);
		for(int j = 0; j < i; j++)
		{
			strcat(interval_str, interval_rows[j]);
		}
	}
	return interval_str;
}

// Funció per donar de baixa un usuari. No l'eliminem de la base de dades, ja que això impediria
// que puguessim mantenir un historial correcte de les partides jugades pels jugadors que abandonen el joc.
// EL que fem s determinar si un usuari est actiu o inactiu amb el flag actiu de la taula jugadors.
int BBDD_deregister_user(char username[USRN_LENGTH], char password[PASS_LENGTH])
{
	char query[QUERY_LENGTH];
	
	strcpy(query, "select jugadors.id from jugadors where jugadors.nom = '");
	strcat(query, username);
	strcat(query, "' and jugadors.passwd = '");
	strcat(query, password);
	strcat(query, "'");
	
	int error = send_query(query);
	
	row = mysql_fetch_row(result);
	
	if (row == NULL)
	{
		// delete FAIL
		return -1;
	}
	else
	{
		int id = atoi(row[0]);
		sprintf(query, "update jugadors set actiu=0 where id=%d", id);
		error = send_query(query);
		return 0;
	}
}

// funció per afegir els resultats d'una partida a la base de dades del joc.
int BBDD_add_game_scores(char name[GAME_LEN], int nPlayers, char** charnames, int* userIds, int* scores, int winnerId, char* initDate, char* endDate, int duration)
{
	char query[QUERY_LENGTH];
	int gameId;
	strcpy(query, "select MAX(partides.id) from partides");
	int error = send_query(query);
	if (error)
		return -1;
	row = mysql_fetch_row(result);
	
	if (row == NULL)
	{
		//no users yet
		gameId = 0;
	}
	else
	{
		gameId = atoi(row[0]) + 1;
	}
	sprintf(query, "insert into partides values (%d, '%s', '%s', '%s', %d, %d)", gameId, name, initDate, endDate, duration, winnerId);
	 error = send_query(query);
	if (error)
		return -1;
	
	for (int i = 0; i < nPlayers; i++)
	{
		sprintf(query, "insert into participants values (%d, %d, '%s', %d)", userIds[i], gameId, charnames[i], scores[i]);
		error = send_query(query);
		if (error)
			return -1;
	}
	return 0;
}


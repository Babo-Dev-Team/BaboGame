#include <stdlib.h>
#include <string.h>
#include <mysql.h>
#include <stdio.h>

int main(int argc, char **argv)
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
}

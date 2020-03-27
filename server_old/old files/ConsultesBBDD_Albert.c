#include <string.h>
#include <stdio.h>
#include <mysql.h>
#include <stdlib.h>

int main (int argc, char **argv)
{
	//Creem els objectes de la base de dades
	MYSQL *conn;
	int err;
	MYSQL_RES *resultat;
	MYSQL_ROW row;
	
	//Creem la connexió amb el MYSQL
	conn = mysql_init(NULL);
	if (conn==NULL)
	{
		printf("Error en la creació de la connexió a la base de dades: %u %s\n", mysql_errno(conn), mysql_error(conn));
		exit(1);
	}
	
	//Accedim a la base de dades
	conn = mysql_real_connect (conn, "localhost", "root", "mysql", "BaboGameBBDD", 0, NULL, 0);
	if (conn==NULL)
	{
		printf("Error en iniciar la connexió: %u %s", mysql_errno(conn), mysql_error(conn));
		exit(1);
	}
	
	//Consulta Demanada - Amb quin personatge a jugat cada jugador en una partida concreta
	char consulta[80];
	int idPartida;
	char partida[5];
	
	printf("Dona'm la ID de una partida perque et doni els personatges utilitzats per cada jugador: \n");
	scanf("%d",&idPartida);
	printf("\n");
	
	strcpy(consulta, "SELECT jugadors.nom, participants.personatge FROM jugadors,participants WHERE (jugadors.id = participants.idJugador) AND (participants.idPartida = '");
	sprintf(partida,"%d",idPartida);
	strcat(consulta,partida);
	strcat(consulta,"')");
	
	err = mysql_query(conn, consulta);
	if(err!=0)
	{
		printf("Error en la execució de la comanda: %u %s", mysql_errno(conn), mysql_error(conn));
		exit(1);
	}
	
	
	resultat = mysql_store_result(conn);
	row = mysql_fetch_row(resultat);
	
	if (row == NULL)
		printf("No ha jugat ningú en aquesta partida o la partida no existeix\n");
	else
	{
		printf("En la partida %d han jugat:\n",idPartida);
		while (row != NULL)
		{
			printf("Jugador:%s ;... Amb el personatge: %s\n",row[0],row[1]);
			row = mysql_fetch_row(resultat);
		}
	}
	
	//Consulta Bonus - Amb quin personatge a jugat cada jugador a les partides d'una data concreta

	char data[20];
	
	printf("\n\nDona'm la data [AAAA-MM-DD] d'una partida perque et doni els personatges utilitzats per cada jugador: \n");
	scanf("%s",data);
	printf("\n\n");
	
	strcpy(consulta, "SELECT jugadors.nom, participants.personatge,partides.dataInici FROM jugadors,participants,partides WHERE (jugadors.id = participants.idJugador) AND (participants.idPartida = partides.id) AND (partides.dataInici LIKE '");
	strcat(consulta,data);
	strcat(consulta,"%') ORDER BY dataInici ASC");
	
	err = mysql_query(conn, consulta);
	if(err!=0)
	{
		printf("Error en la execució de la comanda: %u %s", mysql_errno(conn), mysql_error(conn));
		exit(1);
	}
	
	
	resultat = mysql_store_result(conn);
	row = mysql_fetch_row(resultat);
	
	if (row == NULL)
		printf("No ha jugat ningú en aquesta data\n");
	else
	{
		printf("En la data %s han jugat:\n",data);
		while (row != NULL)
		{
			printf("Jugador:%s || Personatge: %s || Data: %s\n",row[0],row[1], row[2]);
			row = mysql_fetch_row(resultat);
		}
	}
	
	//Tancar la connexió
	mysql_close(conn);
	exit(0);
	
}

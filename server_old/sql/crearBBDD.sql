drop database if exists BaboGameBBDD;
create database BaboGameBBDD;

use BaboGameBBDD;

create table jugadors(
id INTEGER,
nom VARCHAR(20),
passwd varchar(20),
primary key (id)
)ENGINE=InnoDB;

create table partides (
id integer,
dataInici datetime,
dataFinal datetime,
duracio integer,
idGuanyador integer,
primary key (id)	
)ENGINE=InnoDB;

create table participants (
idJugador int,
idPartida int,
personatge varchar(16),
puntsJugador int,
foreign key (idJugador) references jugadors(id),
foreign key (idPartida) references partides(id)
)ENGINE=InnoDB;

insert into jugadors values (1, 'Marc', '1234');
insert into jugadors values (2, 'Maria', 'admin');
insert into jugadors values (3, 'Joan', 'Joan');
insert into jugadors values (4, 'Laia', 'Bcn92');

insert into partides values (1, '2020-01-03 10:02:32', '2020-01-03 10:32:42', 1860, 4);
insert into partides values (2, '2020-01-04 12:42:32', '2020-01-04 13:03:34', 3062, 4);
insert into partides values (3, '2020-01-05 23:34:46', '2020-01-05 23:55:02', 1216, 4);
insert into partides values (4, '2020-01-06 21:34:46', '2020-01-06 22:12:03', 2237, 2);

insert into participants values (1, 1, 'Babo_1', 1000);
insert into participants values (3, 1, 'Babo_1', 2000);
insert into participants values (4, 1, 'Babo_2', 3000);

insert into participants values (2, 2, 'Babo_3', 2000);
insert into participants values (4, 2, 'Babo_2', 4000);

insert into participants values (1, 3, 'Babo_3', 2000);
insert into participants values (2, 3, 'Babo_3', 2000);
insert into participants values (3, 3, 'Babo_1', 3500);
insert into participants values (4, 3, 'Babo_2', 4500);

insert into participants values (1, 4, 'Babo_1', 500);
insert into participants values (2, 4, 'Babo_2', 2500);
insert into participants values (3, 4, 'Babo_2', 1500);
insert into participants values (4, 4, 'Babo_3', 1100);

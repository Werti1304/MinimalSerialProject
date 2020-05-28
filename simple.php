<?php

$dbhost = "localhost";
$dbuser = "root";
$dbpass = "";
$dbname = "minimalserialdatabase";

$dbh = new mysqli($dbhost, $dbuser, $dbpass, $dbname);
 
 if ($dbh->connect_error) 
 {
     die("Connection failed: " . $dbh->connect_error);
 }

$sql = "SELECT Temperature, Time, reading_id FROM temperatures ORDER BY reading_id";

foreach($dbh->query($sql) as $row)
{
        $id = $row["reading_id"];
        $time = date( 'd.m.y h:i', strtotime($row["Time"]));
        $temperature = $row["Temperature"];
        print $id . " | " . $time . " | " . $temperature . "°C<br>";
}
?>
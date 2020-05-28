<?php

$dbhost = "localhost";
$dbuser = "root";
$dbpass = "";
$dbname = "minimalserialdatabase";

$conn = mysqli_connect ($dbhost, $dbuser, $dbpass, $dbname);

if (!$conn) 
    die ("Connection failed: ".mysqli_connect_error ());

$sqlq = "SELECT     * 
         FROM       temperatures";

$row_result = mysqli_query ($conn, $sqlq);
$row        = mysqli_fetch_assoc ($row_result);

echo $row['reading_id']." | ".$row['Temperature'];

?>
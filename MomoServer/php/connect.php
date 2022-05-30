<?php
$host = "localhost";
$user = "root";
$pw = "0523";
$dbName = "momo_db";

$con = mysqli_connect($host, $user, $pw, $dbName);
if (mysqli_connect_errno($con))
	echo 'Connection Error: '.mysqli_connect_error();
?>
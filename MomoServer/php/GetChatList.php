<?php
include("connect.php");

$room_id = $_POST['room_id'];

$query = "select * from momo_db.chat where room_id='$room_id'";
$result = mysqli_query($con, $query);

if (mysqli_num_rows($result))
{
	$n = 0;	
	while ($db = mysqli_fetch_assoc($result))
	{
		$response[$n] = $db;
		++$n;
	}
}

mysqli_close($con);
echo json_encode($response);
?>	
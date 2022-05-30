<?php
include("connect.php");

$person_id = $_POST['person_id'];
$token = $_POST['token'];

$query = "select token from momo_db.person where p_id='$person_id'";
$result = mysqli_query($con, $query);

if (mysqli_num_rows($result))
{
	$db = mysqli_fetch_assoc($result);
	if ($db['token'] != $token)
	{
		$query = "insert into momo_db.person (p_id, token) value ('$person_id','$token') on duplicate key update token='$token'";
		$response = mysqli_query($con, $query);
	}
	else
		$response = true;
}

mysqli_close($con);
echo json_encode($response);
?>
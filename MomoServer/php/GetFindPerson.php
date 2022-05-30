<?php
include("connect.php");

$google_id = $_POST['google_id'];
$google_email = $_POST['google_email'];
$phone_num = $_POST['phone_num'];

$query = "select * from momo_db.person where (phone_num='$phone_num' and google_id='$google_id') or phone_num='$phone_num'";

$result = mysqli_query($con, $query);
$response = mysqli_fetch_assoc($result);

if($response)
{
	if(empty($response['google_id']) || $response['google_id'] != $google_id)
	{
		$person_id = $response['p_id'];
		$query = "update momo_db.person set google_id='$google_id', google_email='$google_email' where p_id='$person_id'";
		mysqli_query($con, $query);
	}
	else if(empty($response['phone_num']) || $response['phone_num'] != $phone_num)
	{
		$person_id = $response['p_id'];
		$query = "update momo_db.person set phone_num='$phone_num' where p_id='$person_id'";
		mysqli_query($con, $query);
	}
}
else
{
	$query = "insert into momo_db.person (phone_num, google_id, google_email) value ('$phone_num', '$google_id', '$google_email')";
	$result = mysqli_query($con, $query);
	
	$success = $result == 1 ? true : false;
	if ($success)
	{
		$insert_id = mysqli_insert_id($con);
		$query = "select * from momo_db.person where p_id='$insert_id'";
		
		$result = mysqli_query($con, $query);
		$response = mysqli_fetch_assoc($result);
	}
}

mysqli_close($con);
echo json_encode($response);
?>
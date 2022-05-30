<?php
include("connect.php");

$room_name = $_POST['room_name'];
$group_id = $_POST['group_id'];
$group_name = $_POST['group_name'];
$person_ids = $_POST['person_ids'];
$person_imgs = $_POST['person_imgs'];
//$person_cnt = $_POST['person_cnt'];

$query = "select id from momo_db.chat_room where group_id='$group_id' and person_ids='$person_ids'";
$result = mysqli_query($con, $query);

if (mysqli_num_rows($result))
{
	$db = mysqli_fetch_assoc($result);
	$response = $db['id'];
}
else
{
	$query = "insert into momo_db.chat_room (name, group_id, group_name, person_ids, person_imgs, update_cnt) value ('$room_name', '$group_id', '$group_name', '$person_ids', '$person_imgs', '1')";
	$result = mysqli_query($con, $query);
	
	$success = $result == 1 ? true : false;
	if ($success)
	{
		$response = mysqli_insert_id($con);
		
		$query = "insert into momo_db.chat_joins (person_id, room_id) value ";
		$split_ids = explode(',', $person_ids);
		$split_length = count($split_ids);
		for ($i = 0; $i < $split_length; $i++)
		{
			$query .= "('$split_ids[$i]', '$response')";
			if ($i < $split_length - 1)
				$query .= ",";
		}
		
		mysqli_query($con, $query);
	}
}

mysqli_close($con);
echo json_encode($response)
?>
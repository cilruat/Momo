<?php
include("connect.php");
include("SendPushNoti.php");

$room_id = $_POST['room_id'];
$room_name = $_POST['room_name'];
$person_id = $_POST['person_id'];
$person_name = $_POST['person_name'];
$person_ids = $_POST['person_ids'];
$group_id = $_POST['group_id'];
$msg = $_POST['msg'];

$query = "insert into momo_db.chat (room_id, person_id, msg) value ('$room_id', '$person_id', '$msg')";
$result = mysqli_query($con, $query);

$success = $result == 1 ? true : false;
if ($success)
{
	$insert_id = mysqli_insert_id($con);
	
	$query = "select * from momo_db.chat where id='$insert_id'";
	$result = mysqli_query($con, $query);
	
	if (mysqli_num_rows($result))
	{
		$db = mysqli_fetch_assoc($result);
		$time = $db['time'];
	}
	
	$query = "update momo_db.chat_room set last_chat_msg='$msg', last_time='$time' where id='$room_id'";
	$result = mysqli_query($con, $query);
}

echo json_encode($db);

// send push
if ($success)
{
	$query = "select p_id, token from momo_db.person where p_id in (select person_id from momo_db.chat_joins where room_id='$room_id')";

	$result = mysqli_query($con, $query);
	if (mysqli_num_rows($result))
	{
		$n = 0;	
		while ($db = mysqli_fetch_assoc($result))
		{
			if ($person_id != $db['p_id'])
			{
				$arrTokens[$n] = $db['token'];
				++$n;
			}
		}
	}

	$title = $person_name;
	$body = $msg;

	$data = array
	(
		'key'			=> 'chat',
		'room_id'		=> $room_id,
		'room_name'		=> $room_name,
		'person_ids'	=> $person_ids,		
		'group_id'		=> $group_id,
		'chat_id'		=> $insert_id,
		'person_id'		=> $person_id,
		'msg'			=> $msg,
		'time'			=> $time
	);

	SendPushNoti($arrTokens, $title, $body, $data);
}

mysqli_close($con);
?>
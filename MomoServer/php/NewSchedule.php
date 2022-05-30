<?php
include("connect.php");
include("SendPushNoti.php");
include("SendSchedule.php");

$group_id = $_POST['group_id'];
$group_name = $_POST['group_name'];
$person_id = $_POST['person_id'];
$person_name = $_POST['person_name'];
$name = $_POST['name'];
$description = $_POST['description'];
$start_date = $_POST['start_date'];
$end_date = $_POST['end_date'];
$color = $_POST['color'];
$alarm_type = $_POST['alarm_type'];
$toggle_push = $_POST['toggle_push'];

$query = "insert into momo_db.schedule (group_id, name, description, start_date, end_date, color, alarm_type) value ('$group_id','$name','$description','$start_date','$end_date','$color','$alarm_type')";

$response = mysqli_query($con, $query);
$success = $response == 1 ? true : false;

// send push server
$alarm_type_int = (int)$alarm_type;
if ($success && $alarm_type_int > 0)
{
	$insert_schedule_id = mysqli_insert_id($con);	
	
	$data = array
	(
		's_id' 		 => $insert_schedule_id,
		'group_id'	 => $group_id,
		'title'		 => $name,
		'desc'		 => $description,
		'start_date' => $start_date,
		'alarm_type' => $alarm_type
	);
	
	post_request($data);
}

// send push
if ($success && $toggle_push == "True")
{
	$query = "select token from momo_db.person where p_id in (select person_id from momo_db.group_join where group_id='$group_id' and person_id not in('$person_id'))";
	//$query = "select token from momo_db.person where p_id in (select person_id from momo_db.group_join where group_id='$group_id')";

	$result = mysqli_query($con, $query);
	if (mysqli_num_rows($result))
	{
		$n = 0;
		while ($token = mysqli_fetch_assoc($result))
		{
			$arrTokens[$n] = $token['token'];
			++$n;
		}
	}

	$title = $group_name;
	$body = $person_name."님이 일정을 등록했습니다.";

	$data = array
	(
		'key'		=> 'schedule',
		'group_id'	=> $group_id
	);

	SendPushNoti($arrTokens, $title, $body, $data);
}

echo json_encode($success);
mysqli_close($con);
?>
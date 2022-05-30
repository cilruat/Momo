<?php
/*define('API_ACCESS_KEY', 'AAAA8VuZ778:APA91bGRd21vt3S6mHmeLs_2yO92_B8kZaBXQ5HPXF5gfoo9xBoLdQXJwkyluEve4hgCtP_pCYbcNQ_tYNAe4iEJbqn4nPSrpIxwoWAuI2PRxNUEBSImDdcvydZjccRZ7L-pMawuUvNN');

$token = array
(
	'csi9V8fHGzI:APA91bGzEd3ylUg_YsuU7VNEg4v8X6yA7h6yb9_qRDHBEwmOsBQnMJ6q1hYONgg4u0QL8e2UrbyrmejQ0EAQXVvfHcs7gIGRjBhXQimVQWV2f_rMFl6QwbDYy0JFayimVFPDksaANiMY'
);

$data = array
(
	'key'			=> 'chat',
	'chat_id'		=> $insert_id,
	'room_id'		=> $room_id,
	'send_id'		=> $send_id,
	'send_name'		=> $send_name,
	'group_name'	=> $group_name,
	'chat_msg'		=> $msg,
	'time'			=> $time
);

$title = '테스트';
$body = '테스트';

$msg = array
(
	'body'			=> $body,
	'title'			=> $title,
	'click_action'	=> 'MAIN'
);

$fields = array
(
	'registration_ids'	=> $token,
	'priority'			=> 'high',	
	'data'				=> $data
);

$headers = array
(
	'Authorization: key='.API_ACCESS_KEY,
	'Content-Type: application/json'
);

$ch = curl_init();
curl_setopt($ch, CURLOPT_URL, 'https://fcm.googleapis.com/fcm/send');
curl_setopt($ch, CURLOPT_POST, true);
curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode($fields));

$result = curl_exec($ch);
curl_close($ch);

include("connect.php");

$query = "update momo_db.chat_room set last_chat_msg='뇸뇸', last_time='2020-10-22 19:23:07' where id='1'";
$result = mysqli_query($con, $query);

mysqli_close($con);
echo json_encode($result);*/
?>
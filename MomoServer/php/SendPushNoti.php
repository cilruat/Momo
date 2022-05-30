<?php
function SendPushNoti($token=array(), $title, $body, $data)
{
	define('API_ACCESS_KEY', 'AAAAAWCXkU4:APA91bH74lcg8twbbpu8i8OJ6K1pL9tK449WzUk5XJD4g8j0yDtGj2CAG1ypVKXZDSk0yoOqDJxDaQdPsymsDWrZ7J_5UHI_lxvXyk8RRaf__gzPE0whCzCsWpkd7HELPuHDFyLMe8Kj');	

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
		'notification'		=> $msg,
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
}
?>
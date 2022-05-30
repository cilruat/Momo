<?php
include("connect.php");
include("SendPushNoti.php");

$notice_id = $_POST['notice_id'];
$person_id = $_POST['person_id'];
$person_name = $_POST['person_name'];
$comment = $_POST['comment'];
$group_id = $_POST['group_id'];
$group_name = $_POST['group_name'];
$receive_id = $_POST['receive_id'];
$parent_id = $_POST['parent_id'];
$tag_comment_id = $_POST['tag_comment_id'];
$tag_person_name = $_POST['tag_person_name'];

$query = "insert into momo_db.comment (parent_id, notice_id, person_id, group_id, description, tag_comment_id, tag_person_name) value ('$parent_id','$notice_id','$person_id','$group_id','$comment','$tag_comment_id','$tag_person_name')";

$response = mysqli_query($con, $query);

$success = $response == 1 ? true : false;
if ($success)
{
	$query = "update momo_db.notice set comment_cnt = comment_cnt+1 where id='$notice_id'";
	mysqli_query($con, $query);
}

echo json_encode($response);

// send push
if ($success)
{
	$query = "select token from momo_db.person where p_id='$receive_id' and person_id not in('$person_id'))";

	$result = mysqli_query($con, $query);
	if (mysqli_num_rows($result))
	{
		$db = mysqli_fetch_assoc($result);
		$arrTokens[0] = $db['token'];
	}

	$title = $group_name;
	$body = $person_name."님이 당신의 글에 댓글을 남겼습니다. \"".$comment."\"";

	$data = array
	(
		'key'		=> 'comment',
		'notice_id'	=> $notice_id
	);

	SendPushNoti($arrTokens, $title, $body, $data);
}

mysqli_close($con);
?>
<?php
include("connect.php");
include("SendPushNoti.php");

$group_id = $_POST['group_id'];
$group_name = $_POST['group_name'];
$person_id = $_POST['person_id'];
$person_name = $_POST['person_name'];
$description = $_POST['description'];
$attach_type = $_POST['attach_type'];
$attach_image_cnt = $_POST['attach_image_cnt'];

if ((int)$attach_image_cnt > 0)
{
	try
	{
		$upload_dir = '../img/group/';	
		$child_folder = $upload_dir."group_".$group_id;
		if (!is_dir($child_folder))
			mkdir($child_folder, 0777, true);
		
		$count = count($_FILES['fileToUpload']['name']);
		for ($i = 0; $i < $count; $i++)
		{
			$files = $_FILES['fileToUpload']['name'][$i];
			//$files = urldecode($files);
			
			$ext = substr($files, strrpos($files, '.') + 1);
			$ch_files = md5(microtime()).'.'.$ext;
			
			$temp_files = $_FILES['fileToUpload']['tmp_name'][$i];
			$file_name = basename($ch_files);
					
			$upload_file = $child_folder."/".$file_name;
			
			$img_type = pathinfo($upload_file, PATHINFO_EXTENSION);
			if ($img_type != "jpg" && $img_type != "png" && $img_type != "jpeg" && $img_type != "gif")
				die ('{"status":"Bad", "reason":"이미지 파일이 아닙니다"}');
			
			if (move_uploaded_file($temp_files, $upload_file) == false)
				die ('{"status":"Bad", "reason":"이미지를 업로드 할 수 없습니다"}');
			
			$attach_images .= $upload_file;
			if ($i < $count - 1)
				$attach_images .= ",";
			
			switch($i)
			{
				case 0: $attach_image_url_1 = $upload_file; break;
				case 1: $attach_image_url_2 = $upload_file; break;
				case 2: $attach_image_url_3 = $upload_file; break;
				case 3: $attach_image_url_4 = $upload_file; break;
			}
		}
	}
	catch(exception $e)
	{
		echo '{"status":"Bad", "reason":"'.$e->getMessage().'"}';
	}
}

//$query = "insert into momo_db.notice (group_id, group_name, person_id, person_name, description, comment_cnt, attach_images, attach_image_cnt) value ('$group_id','$group_name','$person_id','$person_name','$description','0','$attach_images','$attach_image_cnt')";
$query = "insert into momo_db.notice (group_id,group_name,person_id,person_name,description,comment_cnt,attach_image_cnt,attach_image_url_1,attach_image_url_2,attach_image_url_3,attach_image_url_4) value ('$group_id','$group_name','$person_id','$person_name','$description','0','$attach_image_cnt','$attach_image_url_1','$attach_image_url_2','$attach_image_url_3','$attach_image_url_4')";
$response = mysqli_query($con, $query);

$success = $response == 1 ? true : false;
if ($success && (int)$attach_image_cnt > 0)
{
	$new_notice_id = mysqli_insert_id($con);
		
	$query = "insert into momo_db.attach_file (group_id, notice_id, url, type) value ";
	$split_imgs = explode(',', $attach_images);
	$split_length = count($split_imgs);
	for ($i = 0; $i < $split_length; $i++)
	{
		$query .= "('$group_id', '$new_notice_id', '$split_imgs[$i]', '$attach_type')";
		if ($i < $split_length - 1)
			$query .= ",";
	}
	
	$response = mysqli_query($con, $query);
}

$success = $response == 1 ? true : false;
if ($success)		
	echo '{"status":"Success", "reason":""}';
else
	echo '{"status":"Bad", "reason":""}';

// send push
$success = $response == 1 ? true : false;
if ($success)
{
	$insert_notice_id = mysqli_insert_id($con);
	
	$query = "select token from momo_db.person where p_id in (select person_id from momo_db.group_join where group_id='$group_id' and person_id not in('$person_id'))";

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
	
	if ((int)$attach_image_cnt > 0)
		$body = $person_name."님이 사진이 첨부된 글을 올렸습니다.";
	else
		$body = $person_name."님이 새글을 올렸습니다.";
	
	if (empty($description) == false)
		$body .= " \"".$description."\"";

	$data = array
	(
		'key'		=> 'notice',
		'notice_id'	=> $insert_notice_id	
	);

	SendPushNoti($arrTokens, $title, $body, $data);
}

mysqli_close($con);
?>
<?php
include("connect.php");

$group_id = $_POST['group_id'];
$notice_id = $_POST['notice_id'];
$description = $_POST['description'];
$attach_type = $_POST['attach_type'];
$attach_image_cnt = $_POST['attach_image_cnt'];
$update_attach_urls = $_POST['update_attach_urls'];
$remove_img_ids = $_POST['remove_img_ids'];
$remove_images = $_POST['remove_images'];

$split_update_attach_url = array();
if (empty($update_attach_urls) == false)
	$split_update_attach_url = explode(',', $update_attach_urls);

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
		
		if (count($split_update_attach_url) < 4)
			$split_update_attach_url.array_push($split_update_attach_url, $upload_file);
	}
	
	for ($i = 0; $i < 4; $i++)
	{
		$url = empty($split_update_attach_url[$i]) ? "" : $split_update_attach_url[$i];
		switch($i)
		{
			case 0: $attach_image_url_1 = $url; break;
			case 1: $attach_image_url_2 = $url; break;
			case 2: $attach_image_url_3 = $url; break;
			case 3: $attach_image_url_4 = $url; break;
		}
	}
	
	/*$split_remove_imgs = explode(',', $remove_images);
	$split_remove_length = count($split_remove_imgs);
	for ($i = 0; $i < $split_remove_length; $i++)
		unlink($split_remove_imgs[$i]);*/
}
catch(exception $e)
{
	echo '{"status":"Bad", "reason":"'.$e->getMessage().'"}';
}

$query = "update momo_db.notice set description='$description',attach_image_cnt='$attach_image_cnt',attach_image_url_1='$attach_image_url_1',attach_image_url_2='$attach_image_url_2',attach_image_url_3='$attach_image_url_3',attach_image_url_4='$attach_image_url_4' where id='$notice_id'";
$response = mysqli_query($con, $query);

$success = $response == 1 ? true : false;
if ($success)
{
	if ($remove_img_ids > 0)
	{
		//$query = "delete from momo_db.attach_file where id in ($remove_img_ids)";
		$query = "update momo_db.attach_file set `delete`=1 where id in ($remove_img_ids)";
		$remove_success = mysqli_query($con, $query);
	}
	
	if ($count > 0)
	{
		$query = "insert into momo_db.attach_file (group_id, notice_id, url, type) value ";
		$split_attach_imgs = explode(',', $attach_images);
		$split_attach_length = count($split_attach_imgs);
		for ($i = 0; $i < $split_attach_length; $i++)
		{
			$query .= "('$group_id', '$notice_id', '$split_attach_imgs[$i]', '$attach_type')";
			if ($i < $split_length - 1)
				$query .= ",";
		}
		
		mysqli_query($con, $query);
	}
}

if ($success)
	echo '{"status":"Success", "reason":""}';
else
	echo '{"status":"Bad", "reason":""}';

mysqli_close($con);
?>
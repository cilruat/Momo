<?php
include("connect.php");

try
{
	$exist_img = $_POST['existImg'];
	$group_id = $_POST['groupId'];
	$name = $_POST['name'];
	$free = $_POST['free'];

	if ($exist_img == 'true')
	{
		$upload_dir = '../img/group/group_profile_';
		$files = $_FILES['fileToUpload']['name'];
		
		$ext = substr($files, strrpos($files, '.') + 1);
		$ch_files = md5(microtime()).'.'.$ext;
		
		$temp_files = $_FILES['fileToUpload']['tmp_name'];
		$file_name = basename($ch_files);
		
		$child_folder = $upload_dir.$group_id;
		$upload_file = $child_folder."/".$file_name;
		
		$img_type = pathinfo($upload_file, PATHINFO_EXTENSION);
		if ($img_type == "gif")
			die ('{"status" : "Bad", "reason" : "gif 파일은 업로드할 수 없습니다"}');
		
		if ($img_type != "jpg" && $img_type != "png" && $img_type != "jpeg")
			die ('{"status" : "Bad", "reason" : "이미지 파일이 아닙니다"}');	
		
		if (!is_dir($child_folder))
			mkdir($child_folder, 0777, true);
		else
		{
			// 디렉토리 모든 파일 삭제
			$handle = opendir($child_folder);
			while($file = readdir($handle))
			{
				if ($file == "." || $file == "..")
					continue;
				
				$desc .= $file.", ";
				unlink($child_folder."/".$file);
			}
			
			closedir($handle);
		}
		
		if (move_uploaded_file($temp_files, $upload_file))
		{
			$query = "update momo_db.group set name='$name', free='$free', profile_url='$upload_file' where id='$group_id'";
			$response = mysqli_query($con, $query);
			
			$success = $response == 1 ? true : false;
			if ($success)
				echo '{"status":"Success", "reason":"'.$upload_file.'"}';
			else
				echo '{"status":"Bad", "reason":"모임 수정에 실패했습니다"}';
		}
		else
			echo '{"status":"Bad", "reason":"모임 이미지를 업로드 할 수 없습니다."}';
	}
	else
	{
		$query = "update momo_db.group set name='$name', free='$free' where id='$group_id'";
		$response = mysqli_query($con, $query);
		
		$success = $response == 1 ? true : false;
		if ($success)
			echo '{"status":"Success", "reason":"'.$upload_file.'"}';
		else
			echo '{"status":"Bad", "reason":"모임 수정에 실패했습니다"}';
	}
}
catch(exception $e)
{
	echo '{"status":"Bad", "reason":"'.$e->getMessage().'"}';
}

mysqli_close($con);
?>
<?php
include("connect.php");

try
{
	$exist_img = $_POST['existImg'];
	$leader_id = $_POST['leader_id'];
	$name = $_POST['name'];
	$personNums = $_POST['personNums'];	
	$free = $_POST['free'];
	
	$split_nums = explode(',', $personNums);
	$personCnt = count($split_nums);
	
	$query = "insert into momo_db.group (leader_id, name, count, free) value ('$leader_id', '$name', '$personCnt', '$free')";
	$response = mysqli_query($con, $query);

	$success = $response == 1 ? true : false;
	
	if ($success)
	{
		$new_group_id = mysqli_insert_id($con);
				
		for ($i = 0; $i < $personCnt; $i++)
		{
			$number = $split_nums[$i];
			
			$query = "select p_id from momo_db.person where phone_num='$number'";
			$result = mysqli_query($con, $query);
			
			$person_id = -1;
			if (mysqli_num_rows($result))
			{
				$db = mysqli_fetch_assoc($result);
				$person_id = $db['p_id'];
			}
			else
			{
				$query = "insert into momo_db.person (phone_num) value ('$split_nums[$i]')";
				$response = mysqli_query($con, $query);
				
				$success = $response == 1 ? true : false;
				if ($success)
					$person_id = mysqli_insert_id($con);
			}
						
			if ($person_id != -1)
			{
				$leader = $person_id == $leader_id ? "1" : "0";
				$query = "insert into momo_db.group_join (person_id, group_id, leader) value ('$person_id', '$new_group_id', '$leader')";
				mysqli_query($con, $query);
			}
		}
		
		if ($exist_img == 'true')
		{
			$upload_dir = '../img/group/group_profile_';
			$files = $_FILES['fileToUpload']['name'];
			
			$ext = substr($files, strrpos($files, '.') + 1);
			$ch_files = md5(microtime()).'.'.$ext;
			
			$temp_files = $_FILES['fileToUpload']['tmp_name'];
			$file_name = basename($ch_files);
			
			$child_folder = $upload_dir.$new_group_id;
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
				$query = "update momo_db.group set profile_url='$upload_file' where id='$new_group_id'";
				mysqli_query($con, $query);
			}
		}
		
		echo '{"status":"Success", "reason":"'.$new_group_id.','.$upload_file.'"}';
	}
	else
		echo '{"status":"Bad", "reason":"모임 생성에 실패했습니다"}';
}
catch(exception $e)
{
	echo '{"status":"Bad", "reason":"'.$e->getMessage().'"}';
}

mysqli_close($con);
?>
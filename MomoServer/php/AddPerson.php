<?php
include("connect.php");

try
{
	$group_id = $_POST['group_id'];
	$personNums = $_POST['personNums'];	
	
	$split_nums = explode(',', $personNums);
	$personCnt = count($split_nums);
				
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
			$query = "insert into momo_db.group_join (person_id, group_id, leader) value ('$person_id', '$group_id', '0')";
			mysqli_query($con, $query);
		}
	}
		
	$query = "update momo_db.group set count = count+$personCnt where id='$group_id'";
	mysqli_query($con, $query);
		
	echo '{"status":"Success", "reason":""}';
}
catch(exception $e)
{
	echo '{"status":"Bad", "reason":"'.$e->getMessage().'"}';
}

mysqli_close($con);
?>
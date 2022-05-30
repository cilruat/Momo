<?php
include("connect.php");

$person_id = $_POST['person_id'];
$group_id = $_POST['group_id'];

$query = "select * from momo_db.chat_room where id in (select room_id from momo_db.chat_joins where person_id='$person_id') and group_id='$group_id' order by last_time desc";
$result = mysqli_query($con, $query);

if (mysqli_num_rows($result))
{
	$n = 0;	
	while ($db = mysqli_fetch_assoc($result))
	{
		$response[$n] = $db;
		++$n;
	}
}

mysqli_close($con);
echo json_encode($response);
?>	
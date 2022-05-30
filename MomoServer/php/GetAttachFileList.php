<?php
include("connect.php");

$group_id = $_POST['group_id'];
$notice_id = $_POST['notice_id'];
$type = $_POST['type'];

$query = "select * from momo_db.attach_file where group_id='$group_id' and notice_id='$notice_id' and type='$type' and `delete`=0";

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
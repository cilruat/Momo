<?php
include("connect.php");

$person_id = $_POST['person_id'];

$query = "select * from momo_db.group where id in (select group_id from momo_db.group_join where person_id='$person_id') or group.free='1'";
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
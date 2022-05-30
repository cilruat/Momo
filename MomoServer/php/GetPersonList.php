<?php
include("connect.php");

$group_id = $_POST['group_id'];

//$query = "select * from momo_db.group_join where group_id='$group_id'";
$query = "select * from momo_db.person where p_id in (select person_id from momo_db.group_join where group_id='$group_id')";

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
<?php
include("connect.php");

$group_id = $_POST['group_id'];
$start_pos = $_POST['start_pos'];

//$query = "select * from momo_db.notice inner join momo_db.person on notice.person_id = person.p_id where notice.group_id='$group_id' order by time desc limit $start_pos,21";
$query = "select * from momo_db.notice inner join momo_db.person on notice.person_id = person.p_id where notice.group_id='$group_id' and notice.`delete`='0' order by time desc limit $start_pos,21";

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
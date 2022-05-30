<?php
include("connect.php");

$person_id = $_POST['person_id'];
$start_pos = $_POST['start_pos'];

//$query = "select * from momo_db.notice inner join momo_db.person on notice.person_id = person.p_id where notice.group_id in (select group_id from momo_db.group_join where person_id='$person_id') order by time desc limit $start_pos,21";
$query = "select * from momo_db.notice inner join momo_db.person on notice.person_id = person.p_id where notice.group_id in (select group_id from momo_db.group_join where person_id='$person_id') and notice.`delete`='0' order by time desc limit $start_pos,21";

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
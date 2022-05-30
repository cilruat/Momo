<?php
include("connect.php");

$person_id = $_POST['person_id'];
$start_pos = $_POST['start_pos'];

//$query = "select * from momo_db.`comment` inner join momo_db.`group` on `comment`.group_id = `group`.id where `comment`.person_id='$person_id' order by time desc limit $start_pos,21";
$query = "select `comment`.id, parent_id, notice_id, `comment`.person_id, `comment`.group_id, `comment`.`time`, `comment`.`description`, tag_comment_id, tag_person_name, `group`.`name`, notice.`description` as notice_desc from momo_db.`comment` inner join momo_db.`group` on `comment`.group_id = `group`.id left join momo_db.notice on `comment`.notice_id = notice.id where `comment`.person_id='$person_id' order by time desc limit $start_pos,21";

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
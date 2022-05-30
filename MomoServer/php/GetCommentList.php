<?php
include("connect.php");

$notice_id = $_POST['notice_id'];

$query = "select * from momo_db.comment inner join momo_db.person on comment.person_id = person.p_id where comment.notice_id='$notice_id'";
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
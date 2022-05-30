<?php
include("connect.php");

$notice_id = $_POST['notice_id'];

//$query = "delete from momo_db.notice where id='$notice_id'";
$query = "update momo_db.notice set `delete`=1 where id='$notice_id'";
$response = mysqli_query($con, $query);

mysqli_close($con);
echo json_encode($response);
?>
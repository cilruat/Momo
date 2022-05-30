<?php
include("connect.php");

$notice_id = $_POST['notice_id'];

$query = "select comment_cnt from momo_db.notice where id='$notice_id'";

$result = mysqli_query($con, $query);
$response = mysqli_fetch_assoc($result);
mysqli_close($con);
echo json_encode($response);
?>
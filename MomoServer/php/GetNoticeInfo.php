<?php
include("connect.php");

$notice_id = $_POST['notice_id'];

$query = "select * from momo_db.notice inner join momo_db.person on notice.person_id = person.p_id where notice.id='$notice_id'";

$result = mysqli_query($con, $query);
$response = mysqli_fetch_assoc($result);
mysqli_close($con);
echo json_encode($response);
?>
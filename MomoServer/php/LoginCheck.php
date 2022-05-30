<?php
include("connect.php");

$phone = $_POST['phone'];
$name = $_POST['name'];

$query = "select * from momo_db.person where phone_num='$phone' and person_name='$name'";
$result = mysqli_query($con, $query);
$response = mysqli_fetch_assoc($result);
mysqli_close($con);
echo json_encode($response);
?>
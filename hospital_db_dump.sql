-- MySQL dump 10.13  Distrib 9.2.0, for macos15.2 (arm64)
--
-- Host: localhost    Database: hospital
-- ------------------------------------------------------
-- Server version	9.2.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `appointments`
--

DROP TABLE IF EXISTS `appointments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `appointments` (
  `pkId` int NOT NULL AUTO_INCREMENT,
  `PatientID` int DEFAULT NULL,
  `PatientName` varchar(255) DEFAULT NULL,
  `AssignedDoctor` int DEFAULT NULL,
  `AppointmentType` varchar(255) DEFAULT NULL,
  `Status` int DEFAULT NULL,
  `AppointmentDateTime` datetime DEFAULT NULL,
  `temperature` decimal(5,2) DEFAULT NULL,
  `pulseRate` int DEFAULT NULL,
  `weight` decimal(5,2) DEFAULT NULL,
  `height` decimal(5,2) DEFAULT NULL,
  `sugarLevel` decimal(5,2) DEFAULT NULL,
  `bloodPressure` varchar(10) DEFAULT NULL,
  `chiefComplaint` text,
  PRIMARY KEY (`pkId`),
  KEY `appointments_ibfk_1` (`PatientID`),
  KEY `appointments_ibfk_2` (`AssignedDoctor`),
  CONSTRAINT `appointments_ibfk_1` FOREIGN KEY (`PatientID`) REFERENCES `patients` (`patientId`) ON DELETE CASCADE,
  CONSTRAINT `appointments_ibfk_2` FOREIGN KEY (`AssignedDoctor`) REFERENCES `doctors` (`doctor_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=174 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `appointments`
--

LOCK TABLES `appointments` WRITE;
/*!40000 ALTER TABLE `appointments` DISABLE KEYS */;
INSERT INTO `appointments` VALUES (147,18,'Jasper Taguiam',34,'Check up',1,'2025-05-07 09:00:00',36.70,75,68.50,172.30,5.40,'120/80','Severe abdominal pain'),(148,18,'Jasper Taguiam',34,'Consultation',1,'2025-05-07 10:00:00',36.70,75,68.50,172.30,5.40,'120/80','Severe abdominal pain'),(149,18,'Jasper Taguiam',34,'Consultation',1,'2025-05-07 07:00:00',20.00,20,20.00,20.00,20.00,'120/20','!!!1!'),(150,18,'Jasper Taguiam',34,'Check up',0,'2025-05-06 17:00:00',20.00,20,20.00,20.00,20.00,'300/20','wad'),(151,18,'Jasper Taguiam',34,'Check up',1,'2025-05-07 11:00:00',20.00,20,20.00,20.00,20.00,'20/20','dasdas'),(155,19,'Roginand Villegas',97,'Check up',1,'2025-05-07 20:00:00',69.00,69,69.00,170.00,69.00,'69/69','yey'),(158,19,'Roginand Villegas',97,'Check up',0,'2025-05-07 21:00:00',12.00,2,21.00,121.00,12.00,'23/24','das'),(159,19,'Roginand Villegas',34,'Check up',0,'2025-05-07 21:00:00',69.00,69,69.00,69.00,69.00,'69/69','dsadasfd'),(160,18,'Jasper Taguiam',34,'Check up',0,'2025-05-08 06:00:00',69.00,69,69.00,69.00,69.00,'69/69','69'),(161,25,'Gil Tabañag',34,'Check up',0,'2025-05-08 12:00:00',69.00,69,69.00,69.00,69.00,'69/69','sd'),(162,18,'Jasper Taguiam',34,'Consultation',0,'2025-05-08 16:00:00',69.00,69,69.00,69.00,69.00,'69/69','test'),(163,18,'Jasper Taguiam',87,'Check up',0,'2025-05-08 20:00:00',69.00,69,69.00,69.00,69.00,'69/69','dsa'),(164,18,'Jasper Taguiam',34,'Check up',0,'2025-05-09 06:00:00',69.00,69,69.00,69.00,69.00,'69/69','dsadsa\nd\nasda]das\n'),(165,18,'Jasper Taguiam',34,'Check up',1,'2025-05-07 10:00:00',36.70,75,68.50,172.30,5.40,'120/80','Severe abdominal pain'),(166,18,'Jasper Taguiam',34,'Check up',0,'2025-05-07 10:00:00',36.70,75,68.50,172.30,5.40,'120/80','Severe abdominal pain'),(167,18,'Jasper Taguiam',34,'Consultation',0,'2025-05-09 00:00:00',69.00,69,69.00,69.00,69.00,'69/69','abc'),(168,18,'Jasper Taguiam',34,'Check up',0,'2025-05-07 10:00:00',36.70,75,68.50,172.30,5.40,'120/80','Severe abdominal pain'),(169,18,'Jasper Taguiam',34,'Check up',0,'2025-05-07 10:00:00',36.70,75,68.50,172.30,5.40,'120/80','Severe abdominal pain'),(170,18,'Jasper Taguiam',34,'Check up',0,'2025-05-09 10:00:00',36.70,75,68.50,172.30,5.40,'120/80','Severe abdominal pain'),(171,18,'Jasper Taguiam',34,'Check up',0,'2025-05-09 22:00:00',36.70,75,68.50,172.30,5.40,'120/80','Severe abdominal pain'),(172,28,'John Carl Atillo',104,'Check up',1,'2025-05-13 20:00:00',1.00,1,1.00,1.00,1.00,'10/10','1'),(173,28,'John Carl Atillo',104,'Check up',0,'2025-05-13 21:00:00',1.00,1,1.00,1.00,1.00,'10/10','1');
/*!40000 ALTER TABLE `appointments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `doctors`
--

DROP TABLE IF EXISTS `doctors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `doctors` (
  `doctor_id` int NOT NULL,
  `specialization` varchar(100) NOT NULL,
  `is_available` tinyint(1) NOT NULL DEFAULT '0',
  `profile_picture` varchar(255) DEFAULT 'avares://HospitalApp/Assets/Doctor/Profile/dr-default.png',
  `signature` varchar(255) NOT NULL DEFAULT 'avares://HospitalApp/Assets/Doctor/Signature/default-signature.png',
  PRIMARY KEY (`doctor_id`),
  CONSTRAINT `doctors_ibfk_1` FOREIGN KEY (`doctor_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `doctors`
--

LOCK TABLES `doctors` WRITE;
/*!40000 ALTER TABLE `doctors` DISABLE KEYS */;
INSERT INTO `doctors` VALUES (34,'Cardiology',1,'avares://HospitalApp/Assets/Doctor/Profile/dr-default.png','dsg_avares://HospitalApp/Assets/Doctor/Signature/default-signature.png'),(87,'Optometrist',1,'avares://HospitalApp/Assets/Doctor/Profile/drp_423147674_1381982329864744_6508795316134513915_n.jpg','dsg_494357397_1053501473349202_8327261814194451010_n.png'),(96,'Dermatologist',0,'avares://HospitalApp/Assets/Doctor/Profile/drp_494823412_1085339733411613_2919142395980321101_n.png','dsg_494815842_1005138448420365_2573387006619369377_n.png'),(97,'Neurologist',0,'avares://HospitalApp/Assets/Doctor/Profile/drp_att.ExHu4w2hBLayzD1q4M5Qu3mtr1AkW2oA6BGFgj5Tpgo.jpg','dsg_signature (2).png'),(103,'Resident',1,'avares://HospitalApp/Assets/Doctor/Profile/drp_494859156_653859850949022_6373718521637519507_n.jpg','dsg_494687031_1766986840904150_5466943424280531183_n.png'),(104,'as',1,'avares://HospitalApp/Assets/Doctor/Profile/drp_423147674_1381982329864744_6508795316134513915_n.jpg','dsg_494357397_1053501473349202_8327261814194451010_n.png');
/*!40000 ALTER TABLE `doctors` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `patient_med_info`
--

DROP TABLE IF EXISTS `patient_med_info`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `patient_med_info` (
  `patientId` int NOT NULL,
  `medical_allergy` text,
  `latex_allergy` tinyint(1) DEFAULT NULL,
  `food_allergy` text,
  `diet` enum('Vegetarian','Vegan','Omnivore','Pescatarian','Keto','Paleo') DEFAULT NULL,
  `exercise` enum('Daily','Occasionally','Rarely','Never') DEFAULT NULL,
  `sleep` enum('7 hours','less than 6 hours','6-7 hours','8+ hours') DEFAULT NULL,
  `smoking` enum('Yes','No','Former smoker') DEFAULT NULL,
  `alcohol` enum('Occasional','Daily','Weekly','Rarely','Never') DEFAULT NULL,
  `current_medications` text,
  `other_allergy` text,
  PRIMARY KEY (`patientId`),
  CONSTRAINT `fk_patient_info` FOREIGN KEY (`patientId`) REFERENCES `patients` (`patientId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `patient_med_info`
--

LOCK TABLES `patient_med_info` WRITE;
/*!40000 ALTER TABLE `patient_med_info` DISABLE KEYS */;
INSERT INTO `patient_med_info` VALUES (18,'Penicillin',0,'None','Omnivore','Daily','less than 6 hours','No','Occasional','Losartan, Robust','None'),(19,'',0,'Pancit Canton, Tuslo Buwa, Shrimp','Vegetarian','Daily','8+ hours','Yes','Occasional','Ceterizine, Robust, Losartan, PornHub, Complete Family',''),(25,'',0,'Sea foods','Vegan','Daily','6-7 hours','No','Occasional','Bilat ni Lynsy',''),(26,'',0,'','Vegan','Rarely','less than 6 hours','No','Occasional','',''),(28,'Amoxicillin, Penicillin',0,'','Vegan','Occasionally','less than 6 hours','No','Occasional','','');
/*!40000 ALTER TABLE `patient_med_info` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `patients`
--

DROP TABLE IF EXISTS `patients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `patients` (
  `patientId` int NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `sex` varchar(255) NOT NULL,
  `address` varchar(255) NOT NULL,
  `bloodtype` varchar(10) NOT NULL,
  `email` varchar(255) NOT NULL,
  `contactNumber` varchar(255) NOT NULL,
  `bday` date NOT NULL DEFAULT '1000-01-01',
  `createdAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `profile_picture` varchar(255) DEFAULT 'avares://HospitalApp/Assets/Patients/Profile/patient-default.png',
  PRIMARY KEY (`patientId`)
) ENGINE=InnoDB AUTO_INCREMENT=30 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `patients`
--

LOCK TABLES `patients` WRITE;
/*!40000 ALTER TABLE `patients` DISABLE KEYS */;
INSERT INTO `patients` VALUES (18,'Jasper Taguiam','Male','Inayawan, Cebu City','AB+','jaspertaguiam7@gmail.com','09505600719','2004-06-25','2025-04-24 10:08:00','avares://HospitalApp/Assets/Patients/Profiles/patient-default.png'),(19,'Roginand Villegas','Male','Ermita, Cebu City','A+','roginandv@gmail.com','09104146278','2004-11-14','2025-05-07 16:50:44','avares://HospitalApp/Assets/Patients/Profiles/pfp_486691238_1029924589048812_1626878758489266182_n.jpg'),(25,'Gil Tabañag','Male','Inayawan, Cebu City','O+','t.gil0409@gmail.com','09063219424','2004-04-09','2025-05-07 19:33:33','avares://HospitalApp/Assets/Patients/Profiles/pfp_att.ExHu4w2hBLayzD1q4M5Qu3mtr1AkW2oA6BGFgj5Tpgo.jpg'),(26,'Erica Echavez','Female','Kamputhaw, Cebu City','O+','erica.echavez13@gmail.com','09222222222','2004-08-29','2025-05-08 13:04:11','avares://HospitalApp/Assets/Patients/Profiles/pfp_Photo on 5-8-25 at 1.02 PM.jpg'),(28,'John Carl Atillo','Male','Inayawan, Cebu City','O+','jcatillo1121@gmail.com','09610859431','2004-03-20','2025-05-08 20:44:41','avares://HospitalApp/Assets/Patients/Profiles/pfp_IMG_4168-removebg-preview.png');
/*!40000 ALTER TABLE `patients` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pharmacy`
--

DROP TABLE IF EXISTS `pharmacy`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pharmacy` (
  `id` int NOT NULL AUTO_INCREMENT,
  `medicine` varchar(255) NOT NULL,
  `price` float NOT NULL,
  `stocks` int NOT NULL,
  `manufacturer` varchar(255) NOT NULL,
  `type` varchar(255) NOT NULL,
  `dosage` float NOT NULL,
  `unit` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pharmacy`
--

LOCK TABLES `pharmacy` WRITE;
/*!40000 ALTER TABLE `pharmacy` DISABLE KEYS */;
INSERT INTO `pharmacy` VALUES (1,'Paracetamol',10.11,975,'Johnson & Johnson','Tablet',500,'mg'),(2,'Amoxicillin',15,48,'Pfizer','Capsule',500,'mg'),(3,'Loperamide',12,872,'GlaxoSmithKline','Tablet',500,'mg'),(4,'Ibuprofen',12.7,71,'JC Gwapo','Tablet',500,'mg'),(5,'Ceirizine',90,11,'Ritemed','Tablet',500,'mg'),(6,'Aspirin',5,310,'Pharma Inc.','Tablet',500,'mg'),(7,'Metformin',23.75,677,'DiabeteCare.','Tablet',500,'mg'),(8,'Simvastatin',16,79,'Ritemed','Tablet',500,'mg'),(9,'Test',16.21,79,'Test','Syrup',500,'mg'),(15,'test1',21,900,'testt','tablet',500,'mg');
/*!40000 ALTER TABLE `pharmacy` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pharmacy_transactions`
--

DROP TABLE IF EXISTS `pharmacy_transactions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pharmacy_transactions` (
  `id` int NOT NULL AUTO_INCREMENT,
  `total_price` float NOT NULL,
  `receipt_path` varchar(255) NOT NULL,
  `transaction_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pharmacy_transactions`
--

LOCK TABLES `pharmacy_transactions` WRITE;
/*!40000 ALTER TABLE `pharmacy_transactions` DISABLE KEYS */;
/*!40000 ALTER TABLE `pharmacy_transactions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `records`
--

DROP TABLE IF EXISTS `records`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `records` (
  `id` int NOT NULL AUTO_INCREMENT,
  `medicalCertificate` varchar(255) DEFAULT NULL,
  `diagnosis` varchar(255) DEFAULT NULL,
  `prescription` varchar(255) DEFAULT NULL,
  `fkPatientId` int DEFAULT NULL,
  `fkAppointmentId` int DEFAULT NULL,
  `fkDoctorId` int NOT NULL,
  `date` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `fk_patient` (`fkPatientId`),
  KEY `fk_doctor` (`fkDoctorId`),
  KEY `fk_appointment` (`fkAppointmentId`),
  CONSTRAINT `fk_appointment` FOREIGN KEY (`fkAppointmentId`) REFERENCES `appointments` (`pkId`) ON DELETE CASCADE,
  CONSTRAINT `fk_doctor` FOREIGN KEY (`fkDoctorId`) REFERENCES `doctors` (`doctor_id`),
  CONSTRAINT `fk_patient` FOREIGN KEY (`fkPatientId`) REFERENCES `patients` (`patientId`)
) ENGINE=InnoDB AUTO_INCREMENT=1026 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `records`
--

LOCK TABLES `records` WRITE;
/*!40000 ALTER TABLE `records` DISABLE KEYS */;
INSERT INTO `records` VALUES (1017,'Records/MedicalCertificates/MC_148_20250507_18_Jasper Taguiam.pdf','Records/Diagnosis/D_148_20250507_18_Jasper Taguiam.pdf','Records/Prescriptions/RX_148_20250507_18_Jasper Taguiam.pdf',18,148,34,'2025-05-07 10:00:00'),(1018,'Records/MedicalCertificates/MC_148_20250507_18_Jasper Taguiam.pdf','Records/Diagnosis/D_148_20250507_18_Jasper Taguiam.pdf','Records/Prescriptions/RX_148_20250507_18_Jasper Taguiam.pdf',18,148,34,'2025-05-07 10:00:00'),(1019,'Records/MedicalCertificates/MC_151_20250507_18_Jasper Taguiam.pdf','Records/Diagnosis/D_151_20250507_18_Jasper Taguiam.pdf','Records/Prescriptions/RX_151_20250507_18_Jasper Taguiam.pdf',18,151,34,'2025-05-07 11:00:00'),(1024,'Records/MedicalCertificates/MC_155_20250507_19_Roginand Villegas.pdf','Records/Diagnosis/D_155_20250507_19_Roginand Villegas.pdf','Records/Prescriptions/RX_155_20250507_19_Roginand Villegas.pdf',19,155,97,'2025-05-07 20:00:00'),(1025,'Records/MedicalCertificates/MC_172_20250513_28_John Carl Atillo.pdf','Records/Diagnosis/D_172_20250513_28_John Carl Atillo.pdf','Records/Prescriptions/RX_172_20250513_28_John Carl Atillo.pdf',28,172,104,'2025-05-13 20:00:00');
/*!40000 ALTER TABLE `records` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `email` varchar(255) NOT NULL,
  `role` enum('admin','doctor','patient') NOT NULL,
  `username` varchar(50) NOT NULL,
  `password` varchar(255) NOT NULL,
  `contact_number` varchar(20) NOT NULL,
  `sex` enum('male','female') NOT NULL,
  `birthday` date DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `email` (`email`),
  UNIQUE KEY `username` (`username`)
) ENGINE=InnoDB AUTO_INCREMENT=105 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (31,'JC Atillo','jcatillo@example.com','admin','adminjc','$2a$11$0WrmbPFj3SiZ9JByzOw/4.9/AumdI1RlIFdysSkH0UO/d16YrmTA6','123-456-7890','male','2004-03-20','2025-04-21 13:25:48','2025-04-21 13:25:48'),(34,'Dr. JC Atillo','jcatillo1121@example.com','doctor','jc','$2a$11$XlWDCcpvYapH2Wl0yZULm.8r4p/p..51CrcX/2RG3wRxP9TQPCjfG','09610859431','male','2004-03-20','2025-04-22 07:24:48','2025-05-07 15:33:47'),(48,'Roginand','roginandv@gmail.com ','admin','admin3','$2a$11$cikKtQRCU14plB6Bq.iomOqUjij3VGHEhlyCyQ29/zqESqfgjtJwW','09123456789','male','0001-01-01','2025-04-30 03:08:33','2025-04-30 03:08:33'),(51,'Roginand','roginandv@gmail.c om ','admin','adm in3','$2a$11$W34gb9hQ2HEvAc7MYGWCzOMhSQYD3qBzePIjEtATtjhsaNbYiqrvq','09123456789','male','0001-01-01','2025-04-30 03:09:39','2025-04-30 03:09:39'),(55,'Roginand','roginandv@gmail.c som ','admin','adm isn3','$2a$11$gNjKVp1A54xeQy8xFCoIru7LbFWhln18OtC7zXXwpLzcZKQi82yji','09123456789','male','0001-01-01','2025-04-30 03:14:09','2025-04-30 03:14:09'),(59,'Roginand','roginandv@gmail .c som ','admin','adm is n3','$2a$11$WAo3cTHL7QCRZwGHloAAoewlnPOgrynamHuRuyOizh6OaOou2rAom','09123456789','male','0001-01-01','2025-04-30 03:15:28','2025-04-30 03:15:28'),(62,'Roginand','roginandv@gma il .c som ','admin','adm is  n3','$2a$11$c5WJFXu8ZqpTlG8I8wl6m.kYLKANo5wqMv74nYmLA4Dh.wXvxF52u','09123456789','male','0001-01-01','2025-04-30 03:16:11','2025-04-30 03:16:11'),(65,'Roginand','roginandv@gma  il .c som ','admin','adm  is  n3','$2a$11$uiDuHPuUej6NMSmKMCrjteWaBzJI6VGAHU81Pwu2Y3lCH6QyLQ58e','09123456789','male','0001-01-01','2025-04-30 03:19:44','2025-04-30 03:19:44'),(68,'Roginand','roginandv@gma  il . c som ','admin','adm  is   n3','$2a$11$WntWtynxo2gN6eUk9z.ZO.NQaD7w0SwYD10dSXIkNKa6pT59ZMQMu','09123456789','male','0001-01-01','2025-04-30 03:23:58','2025-04-30 03:23:58'),(71,'Roginand','roginandv@gma  il .  c som ','admin','adm  is    n3','$2a$11$OJDEgmj0.dnw5ABlz6wmwepO9/yXiL4tTzvDEG2faGTZqUVNsVd3.','09123456789','male','0001-01-01','2025-04-30 03:27:46','2025-04-30 03:27:46'),(72,'Roginand','roginandv@gmail.csom','admin','admisn3','$2a$11$yTdF5T8KZX7Zm.hfWBn3MeEYI9hsTXFGKSZmCY9zk8g6YVLcK2icS','09123456789','male','0001-01-01','2025-04-30 03:30:35','2025-04-30 03:30:35'),(73,'Roginand','roginandv@gmail.csos','admin','admisn3###','$2a$11$otdHnpslNOPg0yIhlEONvuqmLnr8XTaBuxB/nsGgAuzq6PEpD7dd6','09123456789','male','0001-01-01','2025-04-30 03:35:39','2025-04-30 03:35:39'),(75,'Roginand','roginandv@gmail.c1sos','admin','admisn2','$2a$11$WZEAG7gWjZ1fr7j5YfpqqOS5PZGI3JSTQfNcRFNqA2.uAg0jowtpi','09123456789','male','0001-01-01','2025-04-30 03:37:30','2025-04-30 03:37:30'),(87,'Dr. Enya Moncayo','moncayoenya@gmail.com','doctor','drenya','$2a$11$5FFpWGST79YnXBA9yAQOb.1CqmWnAIQ2m0.SeXvuJLQI3gB33NFvm','09224045682','male','2004-05-09','2025-05-05 17:04:23','2025-05-05 17:04:23'),(96,'Dr. Ramie Theofil ','ramiepondar11@gmail.com','doctor','drtheo','$2a$11$RRnT2E/XAhDlH4j0ZGrFV.keFWm9oBx5AGVKdzlYbg.GOEOvKHA3.','09509574454','male','2004-01-11','2025-05-07 11:20:22','2025-05-07 11:20:22'),(97,'Dr. Lynsy Marie Soronio','lynsymariesoronio05@gmail.com','doctor','drlynsy','$2a$11$mSB5R6FKERhv2de88GGvA.VgpWPTDxHYlxpPPh0O9KJAK0RSygXBu','09286534252','female','2005-07-05','2025-05-07 11:45:39','2025-05-07 11:45:39'),(103,'Dr. Adamusa Pingay','adamspingay@gmail.com','doctor','dradam','$2a$11$QgYB/g2gLBCbTb0kjteotuaxvpUzpYEch9Di3PUShRsf9OjsgrFOy','09498757061','male','2005-10-15','2025-05-08 12:47:27','2025-05-08 12:47:27'),(104,'Dr. test','test@gmail.com','doctor','test1','$2a$11$n1iZNVQoCClIR/eAsS108.bDDNOgzIQ.dYa0noVURnYUcDmwrZKxm','09123456789','male','2025-05-28','2025-05-13 11:18:51','2025-05-13 11:18:51');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-05-13 19:26:03

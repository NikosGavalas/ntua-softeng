
SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `categories`
-- ----------------------------
CREATE TABLE `categories` (
  `category_id` int(11) NOT NULL DEFAULT '0',
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`category_id`),
  UNIQUE KEY `search` (`category_id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of categories
-- ----------------------------
INSERT INTO `categories` VALUES ('0', 'Sports');
INSERT INTO `categories` VALUES ('1', 'Playgrounds');
INSERT INTO `categories` VALUES ('2', 'Educational');

-- ----------------------------
-- Table structure for `event_attendance`
-- ----------------------------
CREATE TABLE `event_attendance` (
  `scheduled_event_id` int(11) NOT NULL,
  `kid_id` int(11) NOT NULL,
  PRIMARY KEY (`scheduled_event_id`,`kid_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for `event_categories`
-- ----------------------------
CREATE TABLE `event_categories` (
  `event_id` int(11) NOT NULL DEFAULT '0',
  `category_id` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`event_id`,`category_id`),
  KEY `search` (`event_id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for `events`
-- ----------------------------
CREATE TABLE `events` (
  `organizer_id` int(10) NOT NULL,
  `event_id` int(10) NOT NULL,
  `title` varchar(255) NOT NULL,
  `description` varchar(1024) DEFAULT NULL,
  `price` int(11) NOT NULL,
  `lat` double DEFAULT NULL,
  `lng` double DEFAULT NULL,
  `address` varchar(255) DEFAULT NULL,
  `duration` int(11) DEFAULT NULL,
  `age_min` int(11) DEFAULT NULL,
  `age_max` int(11) DEFAULT NULL,
  `genders` int(11) DEFAULT '3',
  PRIMARY KEY (`event_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for `kids`
-- ----------------------------
CREATE TABLE `kids` (
  `kid_id` int(11) NOT NULL AUTO_INCREMENT,
  `parent_id` int(11) NOT NULL,
  `name` varchar(255) DEFAULT NULL,
  `birthday` date DEFAULT NULL,
  `gender` int(11) DEFAULT NULL,
  PRIMARY KEY (`kid_id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;


-- ----------------------------
-- Table structure for `scheduled_events`
-- ----------------------------
CREATE TABLE `scheduled_events` (
  `scheduled_event_id` int(11) NOT NULL AUTO_INCREMENT,
  `event_id` int(11) DEFAULT NULL,
  `next_time` datetime DEFAULT NULL,
  `recurrence` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`scheduled_event_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;


-- ----------------------------
-- Table structure for `users`
-- ----------------------------
CREATE TABLE `users` (
  `user_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `email` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `salt` varchar(255) NOT NULL,
  `full_name` varchar(255) DEFAULT NULL,
  `role` int(11) NOT NULL,
  `credits` int(11) NOT NULL,
  `address` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of users
-- ----------------------------
INSERT INTO `users` VALUES ('0', 'admin@admin.com', '3126f568429385bb', 'vlimBKHxCsZ9QhH66MCrlsFAPWQ56HhK', 'admin', '3', '0', null);

-- ----------------------------
-- Function structure for `DISTANCE`
-- ----------------------------
DROP FUNCTION IF EXISTS `DISTANCE`;
DELIMITER ;;
CREATE DEFINER=`haath`@`%` FUNCTION `DISTANCE`(`lat1` double,`lng1` double,`lat2` double,`lng2` double) RETURNS double
BEGIN
	#Calculates the distance in meters between two geographical points
	DECLARE dLng, dLat, a, c DOUBLE;

	SET dLng = RADIANS(lng1 - lng2);
	SET dLat = RADIANS(lat1 - lat2);
	SET a = SIN(dLat / 2.0) * SIN(dLat / 2.0) + COS(RADIANS(lat1)) * COS(RADIANS(lat2)) * SIN(dLng / 2.0) * SIN(dLng / 2.0);
	SET c = 2.0 * ATAN2(SQRT(a), SQRT(1 - a));

	RETURN 6371000.0 * c;
END
;;
DELIMITER ;

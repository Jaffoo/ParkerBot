/*
 Navicat Premium Data Transfer

 Source Server         : 11111
 Source Server Type    : SQLite
 Source Server Version : 3035005
 Source Schema         : main

 Target Server Type    : SQLite
 Target Server Version : 3035005
 File Encoding         : 65001

 Date: 24/04/2023 15:48:44
*/

PRAGMA foreign_keys = false;

-- ----------------------------
-- Table structure for config
-- ----------------------------
DROP TABLE IF EXISTS "config";
CREATE TABLE "config" (
  "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Name" TEXT NOT NULL,
  "Key" TEXT NOT NULL,
  "Value" TEXT NOT NULL,
  "ParentId" INTEGER NOT NULL DEFAULT 0
);

-- ----------------------------
-- Records of config
-- ----------------------------
INSERT INTO "config" VALUES (1, 'Mirai配置', 'Mirai', 'Mirai', 0);
INSERT INTO "config" VALUES (2, 'Mirai安装目录', 'Path', '', 1);
INSERT INTO "config" VALUES (3, 'QQ号', 'QQNum', '', 1);
INSERT INTO "config" VALUES (4, 'Mirai地址', 'Address', '', 1);
INSERT INTO "config" VALUES (5, '验证Key', 'VerifyKey', '', 1);
INSERT INTO "config" VALUES (6, 'QQ启用功能', 'QQFuncEnable', '', 14);
INSERT INTO "config" VALUES (7, '权限功能', 'QQFuncAdmin', '', 14);
INSERT INTO "config" VALUES (8, '用户功能', 'QQFuncUser', '', 14);
INSERT INTO "config" VALUES (9, 'QQ功能列表', 'QQFuncList', '', 0);
INSERT INTO "config" VALUES (10, '闲聊', 'XL', '1', 9);
INSERT INTO "config" VALUES (11, '天气', 'TQ', '2', 9);
INSERT INTO "config" VALUES (12, '看图', 'KT', '3', 9);
INSERT INTO "config" VALUES (13, '基础', 'BaseConfig', '0', 0);
INSERT INTO "config" VALUES (14, 'QQ', 'QQ', 'true', 13);
INSERT INTO "config" VALUES (15, '微博', 'WB', 'true', 13);
INSERT INTO "config" VALUES (16, 'B站', 'BZ', 'true', 13);
INSERT INTO "config" VALUES (17, '口袋48', 'KD', 'true', 13);
INSERT INTO "config" VALUES (18, '小红书', 'XHS', 'true', 13);
INSERT INTO "config" VALUES (19, '抖音', 'DY', 'true', 13);
INSERT INTO "config" VALUES (20, '百度', 'BD', 'true', 13);
INSERT INTO "config" VALUES (21, '群', 'Group', '', 14);
INSERT INTO "config" VALUES (22, '超管', 'Admin', '', 14);
INSERT INTO "config" VALUES (23, '管理', 'Permission', '', 14);
INSERT INTO "config" VALUES (24, '敏感词', 'Sensitive', '', 14);
INSERT INTO "config" VALUES (25, '敏感词操作', 'Action', '', 14);
INSERT INTO "config" VALUES (26, 'Url', 'Url', '', 15);
INSERT INTO "config" VALUES (27, '监听间隔', 'TimeSpan', '3', 15);
INSERT INTO "config" VALUES (28, '转发群', 'Group', '', 15);
INSERT INTO "config" VALUES (29, '是否转发', 'Forward', 'false', 15);
INSERT INTO "config" VALUES (30, 'Url', 'Url', '', 16);
INSERT INTO "config" VALUES (31, '监听间隔', 'TimeSpan', '3', 16);
INSERT INTO "config" VALUES (32, '转发群', 'Group', '', 16);
INSERT INTO "config" VALUES (33, '是否转发', 'Forward', 'false', 16);
INSERT INTO "config" VALUES (34, '账号', 'Account', '', 17);
INSERT INTO "config" VALUES (35, 'Token', 'Token', '', 17);
INSERT INTO "config" VALUES (36, '服务ID', 'ServerId', '', 17);
INSERT INTO "config" VALUES (37, '转发群', 'Group', '', 17);
INSERT INTO "config" VALUES (38, '是否转发', 'Forward', '', 17);
INSERT INTO "config" VALUES (39, 'Url', 'Url', '', 18);
INSERT INTO "config" VALUES (40, '间隔时间', 'TimeSpan', '3', 18);
INSERT INTO "config" VALUES (41, '转发群', 'Group', '', 18);
INSERT INTO "config" VALUES (42, '是否转发', 'Forward', 'false', 18);
INSERT INTO "config" VALUES (43, 'Url', 'Url', '', 19);
INSERT INTO "config" VALUES (44, '间隔时间', 'TimeSpan', '3', 19);
INSERT INTO "config" VALUES (45, '转发群', 'Group', '', 19);
INSERT INTO "config" VALUES (46, '是否转发', 'Forward', 'true', 19);
INSERT INTO "config" VALUES (47, 'AppKey', 'AppKey', '', 20);
INSERT INTO "config" VALUES (48, 'AppSeret', 'AppSeret', '', 20);

-- ----------------------------
-- Table structure for idol
-- ----------------------------
DROP TABLE IF EXISTS "idol";
CREATE TABLE "idol" (
  "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Name" TEXT NOT NULL,
  "Code" TEXT NOT NULL,
  "RoomId" text NOT NULL,
  "Account" TEXT NOT NULL,
  "ServerId" INTEGER NOT NULL,
  "Team" TEXT NOT NULL
);

-- ----------------------------
-- Records of idol
-- ----------------------------

-- ----------------------------
-- Table structure for message
-- ----------------------------
DROP TABLE IF EXISTS "message";
CREATE TABLE "message" (
  "Id" integer NOT NULL PRIMARY KEY AUTOINCREMENT,
  "FromId" integer NOT NULL,
  "FromName" TEXT NOT NULL,
  "ToId" INTEGER NOT NULL,
  "ToName" TEXT NOT NULL,
  "Content" TEXT,
  "Img" TEXT,
  "Voice" TEXT,
  "Video" TEXT,
  "Other" TEXT,
  "CreateDate" DATE NOT NULL,
  "Type" TEXT NOT NULL DEFAULT 1
);

-- ----------------------------
-- Records of message
-- ----------------------------
/*
 Navicat Premium Data Transfer

 Source Server         : config_1
 Source Server Type    : SQLite
 Source Server Version : 3035005
 Source Schema         : main

 Target Server Type    : SQLite
 Target Server Version : 3035005
 File Encoding         : 65001

 Date: 13/07/2023 16:54:16
*/

PRAGMA foreign_keys = false;

-- ----------------------------
-- Table structure for cache
-- ----------------------------
DROP TABLE IF EXISTS "cache";
CREATE TABLE "cache" (
  "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Content" TEXT NOT NULL,
  "Type" integer NOT NULL,
  "CreateDate" DATE NOT NULL
);

-- ----------------------------
-- Records of cache
-- ----------------------------

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
INSERT INTO "config" VALUES (6, 'QQ启用功能', 'FuncEnable', '', 14);
INSERT INTO "config" VALUES (7, '权限功能', 'FuncAdmin', '', 14);
INSERT INTO "config" VALUES (8, '用户功能', 'FuncUser', '', 14);
INSERT INTO "config" VALUES (9, 'QQ功能列表', 'QQFuncList', '', 0);
INSERT INTO "config" VALUES (13, '基础', 'BaseConfig', '0', 0);
INSERT INTO "config" VALUES (14, 'QQ', 'QQ', 'false', 13);
INSERT INTO "config" VALUES (15, '微博', 'WB', 'false', 13);
INSERT INTO "config" VALUES (16, 'B站', 'BZ', 'false', 13);
INSERT INTO "config" VALUES (17, '口袋48', 'KD', 'false', 13);
INSERT INTO "config" VALUES (18, '小红书', 'XHS', 'false', 13);
INSERT INTO "config" VALUES (19, '抖音', 'DY', 'false', 13);
INSERT INTO "config" VALUES (20, '百度', 'BD', 'false', 13);
INSERT INTO "config" VALUES (21, '群', 'Group', '', 14);
INSERT INTO "config" VALUES (22, '超管', 'Admin', '', 14);
INSERT INTO "config" VALUES (23, '管理', 'Permission', '', 14);
INSERT INTO "config" VALUES (24, '敏感词', 'Sensitive', '', 14);
INSERT INTO "config" VALUES (25, '敏感词操作', 'Action', '', 14);
INSERT INTO "config" VALUES (26, 'Url', 'Url', '', 15);
INSERT INTO "config" VALUES (27, '监听间隔', 'TimeSpan', '3', 15);
INSERT INTO "config" VALUES (28, '转发群', 'Group', '', 15);
INSERT INTO "config" VALUES (29, '是否转发', 'ForwardGroup', 'false', 15);
INSERT INTO "config" VALUES (30, 'Url', 'Url', ' ', 16);
INSERT INTO "config" VALUES (31, '监听间隔', 'TimeSpan', '3', 16);
INSERT INTO "config" VALUES (32, '转发群', 'Group', '', 16);
INSERT INTO "config" VALUES (33, '是否转发', 'ForwardGroup', 'false', 16);
INSERT INTO "config" VALUES (34, '账号', 'Account', '', 17);
INSERT INTO "config" VALUES (35, 'Token', 'Token', '', 17);
INSERT INTO "config" VALUES (36, '服务ID', 'ServerId', '', 17);
INSERT INTO "config" VALUES (37, '转发群', 'Group', '', 17);
INSERT INTO "config" VALUES (38, '是否转发', 'ForwardGroup', 'false', 17);
INSERT INTO "config" VALUES (39, 'Url', 'Url', '', 18);
INSERT INTO "config" VALUES (40, '间隔时间', 'TimeSpan', '3', 18);
INSERT INTO "config" VALUES (41, '转发群', 'Group', '', 18);
INSERT INTO "config" VALUES (42, '是否转发', 'ForwardGroup', 'false', 18);
INSERT INTO "config" VALUES (43, 'Url', 'Url', '', 19);
INSERT INTO "config" VALUES (44, '间隔时间', 'TimeSpan', '3', 19);
INSERT INTO "config" VALUES (45, '转发群', 'Group', '', 19);
INSERT INTO "config" VALUES (46, '是否转发', 'ForwardGroup', 'false', 19);
INSERT INTO "config" VALUES (47, 'AppKey', 'AppKey', '', 20);
INSERT INTO "config" VALUES (48, 'AppSeret', 'AppSeret', '', 20);
INSERT INTO "config" VALUES (49, '标准图片', 'ImageList', '', 20);
INSERT INTO "config" VALUES (50, '相似度', 'Similarity', '70', 20);
INSERT INTO "config" VALUES (51, '保存云盘', 'SaveAliyunDisk', 'false', 20);
INSERT INTO "config" VALUES (52, '审核相似度', 'Audit', '50', 20);
INSERT INTO "config" VALUES (53, '相册名称', 'AlbumName', '', 20);
INSERT INTO "config" VALUES (54, '转发好友', 'ForwardQQ', 'false', 15);
INSERT INTO "config" VALUES (55, '好友', 'QQ', '', 15);
INSERT INTO "config" VALUES (56, '转发好友', 'ForwardQQ', 'false', 16);
INSERT INTO "config" VALUES (57, '好友', 'QQ', '', 16);
INSERT INTO "config" VALUES (58, '转发好友', 'ForwardQQ', 'false', 17);
INSERT INTO "config" VALUES (59, '好友', 'QQ', '', 17);
INSERT INTO "config" VALUES (60, '转发好友', 'ForwardQQ', 'false', 18);
INSERT INTO "config" VALUES (61, 'QQ', 'QQ', '', 18);
INSERT INTO "config" VALUES (62, '转发好友', 'ForwardQQ', 'false', 19);
INSERT INTO "config" VALUES (63, 'QQ', 'QQ', '', 19);
INSERT INTO "config" VALUES (64, '人脸识别', 'FaceVerify', 'false', 20);
INSERT INTO "config" VALUES (65, 'AppKey', 'AppKey', 'NjMyZmVmZjFmNGM4Mzg1NDFhYjc1MTk1ZDFjZWIzZmE=', 17);
INSERT INTO "config" VALUES (66, '启用机器人', 'UseMirai', 'false', 1);
INSERT INTO "config" VALUES (67, '图片域名', 'ImgDomain', 'https://source3.48.cn', 17);
INSERT INTO "config" VALUES (68, '视频域名', 'MP4Domain', 'https://mp4.48.cn', 17);
INSERT INTO "config" VALUES (69, '房间ID', 'LiveRoomId', '', 17);
INSERT INTO "config" VALUES (84, '监听消息类型', 'MsgType', '', 17);
INSERT INTO "config" VALUES (85, '消息类型', 'MsgTypeList', '[{
    "name":"文本消息",
    "value":"text"
},
{
    "name":"图片消息",
    "value":"image"
},
{
    "name":"表情消息",
    "value":"EXPRESSIMAGE"
},
{
    "name":"视频消息",
    "value":"video"
},
{
    "name":"音频消息",
    "value":"audio"
},
{
    "name":"回复消息",
    "value":"REPLY"
},
{
    "name":"礼物回复消息",
    "value":"GIFTREPLY"
},
{
    "name":"计分消息",
    "value":"fen"
},
{
    "name":"开播",
    "value":"LIVEPUSH"
},
{
    "name":"房间电台",
    "value":"TEAM_VOICE"
},
{
    "name":"开播/房间电台艾特全体成员",
    "value":"AtAll"
},
{
    "name":"文字翻牌",
    "value":"FLIPCARD"
},
{
    "name":"语音翻牌",
    "value":"FLIPCARD_AUDIO"
},
{
    "name":"视频翻牌",
    "value":"FLIPCARD_VIDEO"
},
{
    "name":"礼物消息",
    "value":"PRESENT_NORMAL"
}]', 17);
INSERT INTO "config" VALUES (86, '吃瓜微博', 'Cg', '', 15);
INSERT INTO "config" VALUES (87, '吃瓜关键字', 'Keyword', '', 15);
INSERT INTO "config" VALUES (88, '姓名', 'Name', '', 17);
INSERT INTO "config" VALUES (89, '程序错误通知', 'Debug', 'false', 14);
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
-- Table structure for log
-- ----------------------------
DROP TABLE IF EXISTS "log";
CREATE TABLE "log" (
  "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Message" TEXT NOT NULL,
  "CreateDate" DATE NOT NULL
);

-- ----------------------------
-- Table structure for message
-- ----------------------------
DROP TABLE IF EXISTS "message";
CREATE TABLE "message" (
  "Id" integer NOT NULL PRIMARY KEY AUTOINCREMENT,
  "MsgId" text NOT NULL,
  "FromId" text NOT NULL,
  "FromName" TEXT NOT NULL,
  "ToId" INTEGER NOT NULL,
  "ToName" TEXT NOT NULL,
  "Content" TEXT,
  "Img" TEXT,
  "Voice" TEXT,
  "Video" TEXT,
  "Other" TEXT,
  "CreateDate" DATE NOT NULL,
  "Type" TEXT NOT NULL DEFAULT 1,
  "MsgType" integer NOT NULL DEFAULT 1
);

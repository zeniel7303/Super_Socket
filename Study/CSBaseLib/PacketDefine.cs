using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSBaseLib
{
    public enum ERROR_CODE : short
    {
        NONE = 0,

        // 서버 초기화 에라
        REDIS_INIT_FAIL = 1,    // Redis 초기화 에러

        // 로그인 
        LOGIN_INVALID_AUTHTOKEN = 1001, // 로그인 실패: 잘못된 인증 토큰
        ADD_USER_DUPLICATION = 1002,
        REMOVE_USER_SEARCH_FAILURE_USER_ID = 1003,
        USER_AUTH_SEARCH_FAILURE_USER_ID = 1004,
        USER_AUTH_ALREADY_SET_AUTH = 1005,
        LOGIN_ALREADY_WORKING = 1006,
        LOGIN_FULL_USER_COUNT = 1007,

        DB_LOGIN_INVALID_PASSWORD = 1011,
        DB_LOGIN_EMPTY_USER = 1012,
        DB_LOGIN_EXCEPTION = 1013,

        ROOM_ENTER_INVALID_STATE = 1021,
        ROOM_ENTER_INVALID_USER = 1022,
        ROOM_ENTER_ERROR_SYSTEM = 1023,
        ROOM_ENTER_INVALID_ROOM_NUMBER = 1024,
        ROOM_ENTER_FAIL_ADD_USER = 1025,
    }

    public enum PACKETID : int
    {
        NOTIFY_CONNECT = 10001,
        NOTIFY_DISCONNECT = 10002,

        REQ_LOGIN,
        RES_LOGIN,

        REQ_ROOM_ENTER,
        RES_ROOM_ENTER,

        REQ_ROOM_LEAVE,
        RES_ROOM_LEAVE,
        NOTIFY_ROOM_LEAVE,

        REQ_ROOM_CHAT,
        BRD_ROOM_CHAT,
    }
}

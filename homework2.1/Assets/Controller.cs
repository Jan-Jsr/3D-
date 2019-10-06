﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour, ISceneController, IUserAction
{
    public LandModel start_land;            //开始陆地
    public LandModel end_land;              //结束陆地
    public BoatModel boat;                  //船
    private RoleModel[] roles;              //角色
    public Judge myJudge;
    UserGUI user_gui;

    public MySceneActionManager actionManager;   //动作管理

    void Start()
    {
        SSDirector director = SSDirector.GetInstance();
        director.CurrentScenceController = this;
        user_gui = gameObject.AddComponent<UserGUI>() as UserGUI;
        LoadResources();

        actionManager = gameObject.AddComponent<MySceneActionManager>() as MySceneActionManager;
    }
    
    public void LoadResources()              //创建水，陆地，角色，船
    {
        GameObject water = Instantiate(Resources.Load("Water", typeof(GameObject)), new Vector3(0,-8,0), Quaternion.identity) as GameObject;
        water.name = "water";
        start_land = new LandModel("start");
        end_land = new LandModel("end");
        boat = new BoatModel();
        roles = new RoleModel[6];
        myJudge = new Judge();

        for (int i = 0; i < 3; i++)
        {
            RoleModel role = new RoleModel("priest");
            role.SetName("priest" + i);
            role.SetPosition(start_land.GetEmptyPosition());
            role.GoLand(start_land);
            start_land.AddRole(role);
            roles[i] = role;
        }

        for (int i = 0; i < 3; i++)
        {
            RoleModel role = new RoleModel("devil");
            role.SetName("devil" + i);
            role.SetPosition(start_land.GetEmptyPosition());
            role.GoLand(start_land);
            start_land.AddRole(role);
            roles[i + 3] = role;
        }
    }
    void Update()
    {
        int start_priest = (start_land.GetRoleNum())[0];
        int start_devil = (start_land.GetRoleNum())[1];
        int end_priest = (end_land.GetRoleNum())[0];
        int end_devil = (end_land.GetRoleNum())[1];

        if (end_priest + end_devil == 6)     //获胜
            myJudge.setStatus(2);

        int[] boat_role_num = boat.GetRoleNumber();
        if (boat.GetBoatSign() == 1)         //在开始岸和船上的角色
        {
            start_priest += boat_role_num[0];
            start_devil += boat_role_num[1];
        }
        else                                  //在结束岸和船上的角色
        {
            end_priest += boat_role_num[0];
            end_devil += boat_role_num[1];
        }
        if (start_priest > 0 && start_priest < start_devil) //失败
        {
            myJudge.setStatus(1);
        }
        if (end_priest > 0 && end_priest < end_devil)        //失败
        {
            myJudge.setStatus(1);
        }
        myJudge.setStatus(0);

    }
    public int Check()
    {
        int start_priest = (start_land.GetRoleNum())[0];
        int start_devil = (start_land.GetRoleNum())[1];
        int end_priest = (end_land.GetRoleNum())[0];
        int end_devil = (end_land.GetRoleNum())[1];

        if (end_priest + end_devil == 6)     //获胜
            return 2;

        int[] boat_role_num = boat.GetRoleNumber();
        if (boat.GetBoatSign() == 1)         //在开始岸和船上的角色
        {
            start_priest += boat_role_num[0];
            start_devil += boat_role_num[1];
        }
        else                                  //在结束岸和船上的角色
        {
            end_priest += boat_role_num[0];
            end_devil += boat_role_num[1];
        }
        if (start_priest > 0 && start_priest < start_devil) //失败
        {
            return 1;
        }
        if (end_priest > 0 && end_priest < end_devil)        //失败
        {
            return 1;
        }
        return 0;                                             //未完成
    }
    /* return myJudge.getStatus();
 }*/
    public void MoveBoat()                  //移动船
    {
        if (boat.IsEmpty() || user_gui.sign != 0) return;
        actionManager.moveBoat(boat.getGameObject(), boat.BoatMoveToPosition(), boat.move_speed);   //动作分离版本改变
        user_gui.sign = Check();
        /*if (user_gui.sign == 1)
        {
            for (int i = 0; i < 3; i++)
            {
                roles[i].PlayGameOver();
                roles[i + 3].PlayGameOver();
            }
        }*///fuck
    }

    public void MoveRole(RoleModel role)    //移动角色
    {
        if (user_gui.sign != 0) return;
        if (role.IsOnBoat())
        {
            LandModel land;
            if (boat.GetBoatSign() == -1)
                land = end_land;
            else
                land = start_land;
            boat.DeleteRoleByName(role.GetName());

            Vector3 end_pos = land.GetEmptyPosition();                                         //动作分离版本改变
            Vector3 middle_pos = new Vector3(role.getGameObject().transform.position.x, end_pos.y, end_pos.z);  //动作分离版本改变
            actionManager.moveRole(role.getGameObject(), middle_pos, end_pos, role.move_speed);  //动作分离版本改变

            role.GoLand(land);
            land.AddRole(role);
        }
        else
        {
            LandModel land = role.GetLandModel();
            if (boat.GetEmptyNumber() == -1 || land.GetLandSign() != boat.GetBoatSign()) return;   //船没有空位，也不是船停靠的陆地，就不上船

            land.DeleteRoleByName(role.GetName());

            Vector3 end_pos = boat.GetEmptyPosition();                                             //动作分离版本改变
            Vector3 middle_pos = new Vector3(end_pos.x, role.getGameObject().transform.position.y, end_pos.z); //动作分离版本改变
            actionManager.moveRole(role.getGameObject(), middle_pos, end_pos, role.move_speed);  //动作分离版本改变

            role.GoBoat(boat);
            boat.AddRole(role);
        }
        user_gui.sign = Check();
        /*if (user_gui.sign == 1)
        {
            for (int i = 0; i < 3; i++)
            {
                roles[i].PlayGameOver();
                roles[i + 3].PlayGameOver();
            }
        }*///fuck
    }

    public void Restart()
    {
        start_land.Reset();
        end_land.Reset();
        boat.Reset();
        for (int i = 0; i < roles.Length; i++)
        {
            roles[i].Reset();
        }
       /* if (user_gui.sign == 1)
        {
            for (int i = 0; i < 3; i++)
            {
                roles[i + 3].PlayIdle();
                roles[i].PlayIdle();
            }
        }*///fuck
    }

   
}

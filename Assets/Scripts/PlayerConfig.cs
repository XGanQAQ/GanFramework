using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "SO/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    /// <summary>
    /// 职业id
    /// </summary>
    public int Id;
    /// <summary>
    /// 职业备注
    /// </summary>
    public string Profession;
    /// <summary>
    /// 生命
    /// </summary>
    public int HitPoint;
    /// <summary>
    /// 近战伤害
    /// </summary>
    public int MeleeDamage;
    /// <summary>
    /// 近战间隔(s)
    /// </summary>
    public float MeleeSpeed;
    /// <summary>
    /// 近战范围(m)
    /// </summary>
    public float MeleeRange;
    /// <summary>
    /// 临时生命
    /// </summary>
    public int TemporaryHitPoint;
    /// <summary>
    /// 耐力(格)
    /// </summary>
    public int Vitality;
    /// <summary>
    /// 耐力恢复时间(s)
    /// </summary>
    public float RecoverTime;
    /// <summary>
    /// 耐力恢复速率(格/s)
    /// </summary>
    public float RecoverEfficiency;
    /// <summary>
    /// 电力
    /// </summary>
    public int ManaPoint;
    /// <summary>
    /// 电能护盾(格)
    /// </summary>
    public int ManaShield;
    /// <summary>
    /// 充能激发时间(s)
    /// </summary>
    public float ActivationTime;
    /// <summary>
    /// 充能效率(格/s)
    /// </summary>
    public float FixEfficiency;
    /// <summary>
    /// 行走速度(m/s)
    /// </summary>
    public float WalkSpeed;
    /// <summary>
    /// 疾跑速度(m/s)
    /// </summary>
    public float RunSpeed;
    /// <summary>
    /// 跳跃高度(m)
    /// </summary>
    public float JumpHeight;
    /// <summary>
    /// 下沉速度(m/s)
    /// </summary>
    public float DiveSpeed;
    /// <summary>
    /// 游泳速度(m/s)
    /// </summary>
    public float SwimSpeed;
    /// <summary>
    /// 加速游泳速度(m/s)
    /// </summary>
    public float FastSwimSpeed;

}

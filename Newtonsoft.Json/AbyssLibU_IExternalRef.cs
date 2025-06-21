namespace AbyssLibU
{
#pragma warning disable IDE1006 // �����X�^�C��
    /// <summary>
    /// �O���Q�Ɨp�C���^�[�t�F�[�X
    /// </summary>
    /// <typeparam name="T">�O���Q�Ƃ���^�BAbyssLibU_IExternalRefRoot�C���^�[�t�F�[�X���K�v�ł��B</typeparam>
    public interface AbyssLibU_IExternalRef<T> where T : AbyssLibU_IExternalRefRoot
    {
        /// <summary>
        /// �O���Q�Ƃ�ݒ肵�܂��B
        /// </summary>
        /// <param name="ExRefRoot">�O���Q�Ƃ��w�肵�܂��B</param>
        public void SetExternalRef(T ExRefRoot);
    }

    /// <summary>
    /// �O���Q�Ɨp�C���^�[�t�F�[�X�i�O���Q�Ƃ�2�̏ꍇ�j
    /// </summary>
    /// <typeparam name="T">�O���Q�Ƃ���^�BAbyssLibU_IExternalRefRoot�C���^�[�t�F�[�X���K�v�ł��B</typeparam>
    /// <typeparam name="T2">�O���Q�Ƃ���^�i����2�j</typeparam>
    public interface AbyssLibU_IExternalRefs<T, T2> where T : AbyssLibU_IExternalRefRoot
    {
        /// <summary>
        /// �O���Q�Ƃ�ݒ肵�܂��B
        /// </summary>
        /// <param name="ExRefRoot">�O���Q�Ɓi����1�j���w�肵�܂��B</param>
        /// <param name="ExRef2">�O���Q�Ɓi����2�j���w�肵�܂��B</param>
        public void SetExternalRefs(T ExRefRoot, T2 ExRef2);
    }

    /// <summary>
    /// �O���Q�Ɨp�C���^�[�t�F�[�X�i�O���Q�Ƃ�2�̏ꍇ�j
    /// </summary>
    /// <typeparam name="T">�O���Q�Ƃ���^�BAbyssLibU_IExternalRefRoot�C���^�[�t�F�[�X���K�v�ł��B</typeparam>
    /// <typeparam name="T2">�O���Q�Ƃ���^�i����2�j</typeparam>
    /// <typeparam name="T3">�O���Q�Ƃ���^�i����3�j</typeparam>
    public interface AbyssLibU_IExternalRefs<T, T2, T3> where T : AbyssLibU_IExternalRefRoot
    {
        /// <summary>
        /// �O���Q�Ƃ�ݒ肵�܂��B
        /// </summary>
        /// <param name="ExRefRoot">�O���Q�Ɓi����1�j���w�肵�܂��B</param>
        /// <param name="ExRef2">�O���Q�Ɓi����2�j���w�肵�܂��B</param>
        /// <param name="ExRef3">�O���Q�Ɓi����3�j���w�肵�܂��B</param>
        public void SetExternalRefs(T ExRefRoot, T2 ExRef2, T3 ExRef3);
    }
#pragma warning restore IDE1006 // �����X�^�C��
}
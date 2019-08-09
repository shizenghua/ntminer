﻿using NTMiner.MinerServer;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NTMiner.Core.MinerServer.Impl {
    public class ColumnsShowSet : IColumnsShowSet {
        private readonly Dictionary<Guid, ColumnsShowData> _dicById = new Dictionary<Guid, ColumnsShowData>();

        private readonly INTMinerRoot _root;
        public ColumnsShowSet(INTMinerRoot root) {
            _root = root;
            VirtualRoot.Window<AddColumnsShowCommand>("添加列显", LogEnum.DevConsole,
                action: (message) => {
                    InitOnece();
                    if (message == null || message.Input == null || message.Input.GetId() == Guid.Empty || message.Input.GetId() == ColumnsShowData.PleaseSelect.Id) {
                        throw new ArgumentNullException();
                    }
                    if (_dicById.ContainsKey(message.Input.GetId())) {
                        return;
                    }
                    ColumnsShowData entity = new ColumnsShowData().Update(message.Input);
                    Server.ControlCenterService.AddOrUpdateColumnsShowAsync(entity, (response, exception) => {
                        if (response.IsSuccess()) {
                            _dicById.Add(entity.Id, entity);
                            VirtualRoot.Happened(new ColumnsShowAddedEvent(entity));
                        }
                        else {
                            Write.UserFail(response.ReadMessage(exception));
                        }
                    });
                });
            VirtualRoot.Window<UpdateColumnsShowCommand>("更新列显", LogEnum.DevConsole,
                action: (message) => {
                    InitOnece();
                    if (message == null || message.Input == null || message.Input.GetId() == Guid.Empty) {
                        throw new ArgumentNullException();
                    }
                    if (!_dicById.ContainsKey(message.Input.GetId())) {
                        return;
                    }
                    ColumnsShowData entity = _dicById[message.Input.GetId()];
                    ColumnsShowData oldValue = new ColumnsShowData().Update(entity);
                    entity.Update(message.Input);
                    Server.ControlCenterService.AddOrUpdateColumnsShowAsync(entity, (response, exception) => {
                        if (!response.IsSuccess()) {
                            entity.Update(oldValue);
                            VirtualRoot.Happened(new ColumnsShowUpdatedEvent(entity));
                            Write.UserFail(response.ReadMessage(exception));
                        }
                    });
                    VirtualRoot.Happened(new ColumnsShowUpdatedEvent(entity));
                });
            VirtualRoot.Window<RemoveColumnsShowCommand>("移除列显", LogEnum.DevConsole,
                action: (message) => {
                    InitOnece();
                    if (message == null || message.EntityId == Guid.Empty || message.EntityId == ColumnsShowData.PleaseSelect.Id) {
                        throw new ArgumentNullException();
                    }
                    if (!_dicById.ContainsKey(message.EntityId)) {
                        return;
                    }
                    ColumnsShowData entity = _dicById[message.EntityId];
                    Server.ControlCenterService.RemoveColumnsShowAsync(entity.Id, (response, exception) => {
                        if (response.IsSuccess()) {
                            _dicById.Remove(entity.Id);
                            VirtualRoot.Happened(new ColumnsShowRemovedEvent(entity));
                        }
                        else {
                            Write.UserFail(response.ReadMessage(exception));
                        }
                    });
                });
        }

        private bool _isInited = false;
        private readonly object _locker = new object();

        private void InitOnece() {
            if (_isInited) {
                return;
            }
            Init();
        }

        private void Init() {
            if (!_isInited) {
                lock (_locker) {
                    if (!_isInited) {
                        var result = Server.ControlCenterService.GetColumnsShows();
                        foreach (var item in result) {
                            if (!_dicById.ContainsKey(item.GetId())) {
                                _dicById.Add(item.GetId(), item);
                            }
                        }
                        if (!_dicById.ContainsKey(ColumnsShowData.PleaseSelect.Id)) {
                            _dicById.Add(ColumnsShowData.PleaseSelect.Id, ColumnsShowData.PleaseSelect);
                            Server.ControlCenterService.AddOrUpdateColumnsShowAsync(ColumnsShowData.PleaseSelect, (response, exception) => {
                                if (!response.IsSuccess()) {
                                    Logger.ErrorDebugLine("AddOrUpdateColumnsShowAsync " + response.ReadMessage(exception));
                                }
                            });
                        }
                        _isInited = true;
                    }
                }
            }
        }

        public bool Contains(Guid columnsShowId) {
            InitOnece();
            return _dicById.ContainsKey(columnsShowId);
        }

        public bool TryGetColumnsShow(Guid columnsShowId, out IColumnsShow columnsShow) {
            InitOnece();
            var r = _dicById.TryGetValue(columnsShowId, out ColumnsShowData g);
            columnsShow = g;
            return r;
        }

        public IEnumerator<IColumnsShow> GetEnumerator() {
            InitOnece();
            return _dicById.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            InitOnece();
            return _dicById.Values.GetEnumerator();
        }
    }
}
